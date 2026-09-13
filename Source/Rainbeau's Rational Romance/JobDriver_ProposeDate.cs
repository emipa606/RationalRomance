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

        var path = new List<IntVec3> { root };
        var current = root;

        for (var i = 0; i < 8; i++)
        {
            if (!TryFindBestNextDateCell(p1, p2, root, current, path, startRadialIndex, endRadialIndex,
                    radialIndexStride, out var nextCell))
            {
                result = null;
                return false;
            }

            path.Add(nextCell);
            current = nextCell;
        }

        path.Add(root);
        result = path;
        return true;
    }

    private static bool TryFindBestNextDateCell(Pawn p1, Pawn p2, IntVec3 root, IntVec3 current,
        List<IntVec3> path, int startRadialIndex, int endRadialIndex, int radialIndexStride, out IntVec3 bestCell)
    {
        bestCell = IntVec3.Invalid;
        var bestScore = -1f;

        for (var j = startRadialIndex; j > endRadialIndex; j -= radialIndexStride)
        {
            var candidate = current + GenRadial.RadialPattern[j];
            if (!IsValidDateCell(candidate, current, p1, p2))
            {
                continue;
            }

            var score = GetDateCellScore(root, current, candidate, path);
            if (!(score > bestScore))
            {
                continue;
            }

            bestCell = candidate;
            bestScore = score;
        }

        return bestScore >= 0f;
    }

    private static bool IsValidDateCell(IntVec3 candidate, IntVec3 current, Pawn p1, Pawn p2)
    {
        return candidate.InBounds(p1.Map) &&
               candidate.Standable(p1.Map) &&
               !candidate.IsForbidden(p1) &&
               !candidate.IsForbidden(p2) &&
               !candidate.GetTerrain(p1.Map).avoidWander &&
               GenSight.LineOfSight(current, candidate, p1.Map) &&
               !candidate.Roofed(p1.Map) &&
               !PawnUtility.KnownDangerAt(candidate, p1.Map, p1) &&
               !PawnUtility.KnownDangerAt(candidate, p1.Map, p2);
    }

    private static float GetDateCellScore(IntVec3 root, IntVec3 current, IntVec3 candidate, List<IntVec3> path)
    {
        var score = 10000f;
        foreach (var point in path)
        {
            score += (point - candidate).LengthManhattan;
        }

        var distanceFromRoot = (candidate - root).LengthManhattan;
        if (distanceFromRoot > 40f)
        {
            score *= Mathf.InverseLerp(70f, 40f, distanceFromRoot);
        }

        if (path.Count >= 2)
        {
            var lastDirection = path[^1] - path[^2];
            var lastAngle = lastDirection.AngleFlat;
            var newAngle = (candidate - current).AngleFlat;
            if (newAngle <= lastAngle)
            {
                lastAngle -= 360f;
            }

            if (newAngle - lastAngle > 110f)
            {
                score *= 0.01f;
            }
        }

        if (path.Count >= 4 && (current - root).LengthManhattan < (candidate - root).LengthManhattan)
        {
            score *= 1E-05f;
        }

        return score;
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
        }
        else
        {
            var list2 = (from c in list
                orderby BeautyUtility.AverageBeautyPerceptible(c, p1.Map) descending
                select c).ToList();
            best = list2.FirstOrDefault();
            list2.Reverse();
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