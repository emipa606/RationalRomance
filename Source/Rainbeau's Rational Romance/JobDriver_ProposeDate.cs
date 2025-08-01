using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RationalRomance_Code;

public class JobDriver_ProposeDate : JobDriver
{
    private bool successfulPass = true;

    private Pawn Actor => GetActor();

    private Pawn TargetPawn => TargetThingA as Pawn;

    private Building_Bed TargetBed => TargetThingB as Building_Bed;

    private static TargetIndex TargetPawnIndex => TargetIndex.A;

    private TargetIndex TargetBedIndex => TargetIndex.B;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    private static bool TryFindUnforbiddenDatePath(Pawn p1, Pawn p2, IntVec3 root, out List<IntVec3> result)
    {
        var startRadialIndex = GenRadial.NumCellsInRadius(14f);
        var endRadialIndex = GenRadial.NumCellsInRadius(2f);
        const int radialIndexStride = 3;
        var intVec3S = new List<IntVec3> { root };
        var intVec3 = root;
        for (var i = 0; i < 8; i++)
        {
            var invalid = IntVec3.Invalid;
            var single1 = -1f;
            for (var j = startRadialIndex; j > endRadialIndex; j -= radialIndexStride)
            {
                var radialPattern = intVec3 + GenRadial.RadialPattern[j];
                if (!radialPattern.InBounds(p1.Map) || !radialPattern.Standable(p1.Map) ||
                    radialPattern.IsForbidden(p1) || radialPattern.IsForbidden(p2) ||
                    radialPattern.GetTerrain(p1.Map).avoidWander ||
                    !GenSight.LineOfSight(intVec3, radialPattern, p1.Map) || radialPattern.Roofed(p1.Map) ||
                    PawnUtility.KnownDangerAt(radialPattern, p1.Map, p1) ||
                    PawnUtility.KnownDangerAt(radialPattern, p1.Map, p2))
                {
                    continue;
                }

                var lengthManhattan = 10000f;
                foreach (var vec3 in intVec3S)
                {
                    lengthManhattan += (vec3 - radialPattern).LengthManhattan;
                }

                float lengthManhattan1 = (radialPattern - root).LengthManhattan;
                if (lengthManhattan1 > 40f)
                {
                    lengthManhattan *= Mathf.InverseLerp(70f, 40f, lengthManhattan1);
                }

                if (intVec3S.Count >= 2)
                {
                    var item = intVec3S[^1] - intVec3S[^2];
                    var angleFlat = item.AngleFlat;
                    var angleFlat1 = (radialPattern - intVec3).AngleFlat;
                    if (angleFlat1 <= angleFlat)
                    {
                        angleFlat -= 360f;
                    }

                    var single = angleFlat1 - angleFlat;

                    if (single > 110f)
                    {
                        lengthManhattan *= 0.01f;
                    }
                }

                if (intVec3S.Count >= 4 &&
                    (intVec3 - root).LengthManhattan < (radialPattern - root).LengthManhattan)
                {
                    lengthManhattan *= 1E-05f;
                }

                if (!(lengthManhattan > single1))
                {
                    continue;
                }

                invalid = radialPattern;
                single1 = lengthManhattan;
            }

            if (single1 < 0f)
            {
                result = null;
                return false;
            }

            intVec3S.Add(invalid);
            intVec3 = invalid;
        }

        intVec3S.Add(root);
        result = intVec3S;
        return true;
    }

    private bool isTargetPawnOkay()
    {
        return !TargetPawn.Dead && !TargetPawn.Downed && TargetPawn.Position.InAllowedArea(GetActor());
    }

    private static bool tryFindMostBeautifulRootInDistance(int distance, Pawn p1, Pawn p2, out IntVec3 best)
    {
        best = default;
        var list = new List<IntVec3>();
        for (var i = 0; i < 200; i++)
        {
            if (CellFinder.TryFindRandomCellNear(p1.Position, p1.Map, distance,
                    c => c.InBounds(p1.Map) && !c.IsForbidden(p1) && !c.IsForbidden(p2) &&
                         p1.CanReach(c, PathEndMode.OnCell, Danger.Some), out var item))
            {
                list.Add(item);
            }
        }

        bool result;
        if (list.Count == 0)
        {
            result = false;
            //Log.Message("No date walk destination found.");
        }
        else
        {
            var list2 = (from c in list
                orderby BeautyUtility.AverageBeautyPerceptible(c, p1.Map) descending
                select c).ToList();
            best = list2.FirstOrDefault();
            list2.Reverse();
            //Log.Message("Date walk destinations found from beauty " +
            //            BeautyUtility.AverageBeautyPerceptible(best, p1.Map) + " to " +
            //            BeautyUtility.AverageBeautyPerceptible(list2.FirstOrDefault(), p1.Map));
            result = true;
        }

        return result;
    }

    [DebuggerHidden]
    public override IEnumerable<Toil> MakeNewToils()
    {
        if (!SexualityUtilities.IsFree(TargetPawn))
        {
            yield break;
        }

        yield return Toils_Goto.GotoThing(TargetPawnIndex, PathEndMode.Touch);
        var AskOut = new Toil();
        AskOut.AddFailCondition(() => !isTargetPawnOkay());
        AskOut.defaultCompleteMode = ToilCompleteMode.Delay;
        AskOut.initAction = delegate
        {
            ticksLeftThisToil = 50;
            FleckMaker.ThrowMetaIcon(GetActor().Position, GetActor().Map, FleckDefOf.Heart);
        };
        yield return AskOut;
        var AwaitResponse = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                successfulPass = SexualityUtilities.IsFree(TargetPawn);
                FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map,
                    successfulPass ? FleckDefOf.Heart : FleckDefOf.IncapIcon);
            }
        };
        AwaitResponse.AddFailCondition(() => !successfulPass);
        yield return AwaitResponse;
        if (successfulPass)
        {
            yield return new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant,
                initAction = delegate
                {
                    var jobDateLead = new Job(RRRJobDefOf.JobDateLead);
                    if (!tryFindMostBeautifulRootInDistance(40, pawn, TargetPawn, out var root))
                    {
                        return;
                    }

                    if (!TryFindUnforbiddenDatePath(pawn, TargetPawn, root, out var list))
                    {
                        return;
                    }

                    //Log.Message("Date walk path found.");
                    jobDateLead.targetQueueB = [];
                    for (var i = 1; i < list.Count; i++)
                    {
                        jobDateLead.targetQueueB.Add(list[i]);
                    }

                    jobDateLead.locomotionUrgency = LocomotionUrgency.Amble;
                    jobDateLead.targetA = TargetPawn;
                    Actor.jobs.jobQueue.EnqueueFirst(jobDateLead);
                    var job2 = new Job(RRRJobDefOf.JobDateFollow)
                    {
                        locomotionUrgency = LocomotionUrgency.Amble, targetA = Actor
                    };
                    TargetPawn.jobs.jobQueue.EnqueueFirst(job2);
                    TargetPawn.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                    Actor.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                }
            };
        }
    }
}