using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace RationalRomance_Code;

public class JobDriver_LeadHookup : JobDriver
{
    private bool WasSuccessfulPass { get; set; } = true;

    private Pawn Actor => GetActor();

    private Pawn TargetPawn => TargetThingA as Pawn;

    private Building_Bed TargetBed => TargetThingB as Building_Bed;

    private static TargetIndex TargetPawnIndex => TargetIndex.A;

    private TargetIndex TargetBedIndex => TargetIndex.B;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    private bool doesTargetPawnAcceptAdvance()
    {
        return SexualityUtilities.IsFree(TargetPawn) && SexualityUtilities.WillPawnTryHookup(TargetPawn) &&
               SexualityUtilities.IsHookupAppealing(TargetPawn, Actor);
    }

    private bool isTargetPawnOkay()
    {
        return !TargetPawn.Dead && !TargetPawn.Downed;
    }

    [DebuggerHidden]
    public override IEnumerable<Toil> MakeNewToils()
    {
        if (!SexualityUtilities.IsFree(TargetPawn))
        {
            yield break;
        }

        // walk to target pawn
        yield return Toils_Goto.GotoThing(TargetPawnIndex, PathEndMode.Touch);

        var TryItOn = new Toil();
        // make sure target is feeling ok
        TryItOn.AddFailCondition(() => !isTargetPawnOkay());
        TryItOn.defaultCompleteMode = ToilCompleteMode.Delay;
        // show heart between pawns
        TryItOn.initAction = delegate
        {
            ticksLeftThisToil = 50;
            FleckMaker.ThrowMetaIcon(Actor.Position, Actor.Map, FleckDefOf.Heart);
        };
        yield return TryItOn;

        var awaitResponse = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                var list = new List<RulePackDef>();
                WasSuccessfulPass = doesTargetPawnAcceptAdvance();
                if (WasSuccessfulPass)
                {
                    FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, FleckDefOf.Heart);
                    list.Add(RRRMiscDefOf.HookupSucceeded);
                }
                else
                {
                    FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, FleckDefOf.IncapIcon);
                    Actor.needs?.mood?.thoughts?.memories?.TryGainMemory(RRRThoughtDefOf.RebuffedMyHookupAttempt,
                        TargetPawn);
                    TargetPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(
                        RRRThoughtDefOf.FailedHookupAttemptOnMe,
                        Actor);
                    list.Add(RRRMiscDefOf.HookupFailed);
                }

                // add "tried hookup with" to the log
                Find.PlayLog.Add(new PlayLogEntry_Interaction(RRRMiscDefOf.TriedHookupWith, pawn, TargetPawn,
                    list));
            }
        };
        awaitResponse.AddFailCondition(() => !WasSuccessfulPass);
        yield return awaitResponse;

        if (WasSuccessfulPass)
        {
            yield return new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant,
                initAction = delegate
                {
                    if (!WasSuccessfulPass)
                    {
                        return;
                    }

                    Actor.jobs.jobQueue.EnqueueFirst(new Job(RRRJobDefOf.DoLovinCasual, TargetPawn,
                        TargetBed, TargetBed.GetSleepingSlotPos(0)));
                    TargetPawn.jobs.jobQueue.EnqueueFirst(new Job(RRRJobDefOf.DoLovinCasual, Actor,
                        TargetBed, TargetBed.GetSleepingSlotPos(1)));
                    // important for 1.1 that the hookup leader ends their job last. best guess is that it's related to the new garbage collection
                    TargetPawn.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                    Actor.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                }
            };
        }
    }
}