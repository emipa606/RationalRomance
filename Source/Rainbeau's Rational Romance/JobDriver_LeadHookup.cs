using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace RationalRomance_Code
{
    public class JobDriver_LeadHookup : JobDriver
    {
        public bool successfulPass = true;

        public bool wasSuccessfulPass => successfulPass;

        private Pawn actor => GetActor();

        private Pawn TargetPawn => TargetThingA as Pawn;

        private Building_Bed TargetBed => TargetThingB as Building_Bed;

        private TargetIndex TargetPawnIndex => TargetIndex.A;

        private TargetIndex TargetBedIndex => TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private bool DoesTargetPawnAcceptAdvance()
        {
            return SexualityUtilities.IsFree(TargetPawn) && SexualityUtilities.WillPawnTryHookup(TargetPawn) &&
                   SexualityUtilities.IsHookupAppealing(TargetPawn, actor);
        }

        private bool IsTargetPawnOkay()
        {
            return !TargetPawn.Dead && !TargetPawn.Downed;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (!SexualityUtilities.IsFree(TargetPawn))
            {
                yield break;
            }

            // walk to target pawn
            yield return Toils_Goto.GotoThing(TargetPawnIndex, PathEndMode.Touch);

            var TryItOn = new Toil();
            // make sure target is feeling ok
            TryItOn.AddFailCondition(() => !IsTargetPawnOkay());
            TryItOn.defaultCompleteMode = ToilCompleteMode.Delay;
            // show heart between pawns
            TryItOn.initAction = delegate
            {
                ticksLeftThisToil = 50;
                FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.Heart);
            };
            yield return TryItOn;

            var AwaitResponse = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Instant,
                initAction = delegate
                {
                    var list = new List<RulePackDef>();
                    successfulPass = DoesTargetPawnAcceptAdvance();
                    if (successfulPass)
                    {
                        FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, FleckDefOf.Heart);
                        list.Add(RRRMiscDefOf.HookupSucceeded);
                    }
                    else
                    {
                        FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, FleckDefOf.IncapIcon);
                        actor.needs?.mood?.thoughts?.memories?.TryGainMemory(RRRThoughtDefOf.RebuffedMyHookupAttempt,
                            TargetPawn);
                        TargetPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(
                            RRRThoughtDefOf.FailedHookupAttemptOnMe,
                            actor);
                        list.Add(RRRMiscDefOf.HookupFailed);
                    }

                    // add "tried hookup with" to the log
                    Find.PlayLog.Add(new PlayLogEntry_Interaction(RRRMiscDefOf.TriedHookupWith, pawn, TargetPawn,
                        list));
                }
            };
            AwaitResponse.AddFailCondition(() => !wasSuccessfulPass);
            yield return AwaitResponse;

            if (wasSuccessfulPass)
            {
                yield return new Toil
                {
                    defaultCompleteMode = ToilCompleteMode.Instant,
                    initAction = delegate
                    {
                        if (!wasSuccessfulPass)
                        {
                            return;
                        }

                        actor.jobs.jobQueue.EnqueueFirst(new Job(RRRJobDefOf.DoLovinCasual, TargetPawn,
                            TargetBed, TargetBed.GetSleepingSlotPos(0)));
                        TargetPawn.jobs.jobQueue.EnqueueFirst(new Job(RRRJobDefOf.DoLovinCasual, actor,
                            TargetBed, TargetBed.GetSleepingSlotPos(1)));
                        // important for 1.1 that the hookup leader ends their job last. best guess is that it's related to the new garbage collection
                        TargetPawn.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                        actor.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                    }
                };
            }
        }
    }
}