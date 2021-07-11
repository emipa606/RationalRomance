using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace RationalRomance_Code
{
    public class JobDriver_DoLovinCasual : JobDriver
    {
        private const int TicksBetweenHeartMotes = 100;
        private const int duration = 20;
        private readonly TargetIndex BedInd = TargetIndex.B;
        private readonly TargetIndex PartnerInd = TargetIndex.A;
        private readonly TargetIndex SlotInd = TargetIndex.C;
        private int ticksLeft = 20;

        private Building_Bed Bed => (Building_Bed) (Thing) job.GetTarget(BedInd);

        private Pawn actor => GetActor();

        private Pawn Partner => (Pawn) (Thing) job.GetTarget(PartnerInd);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksLeft, "ticksLeft");
        }

        private IntVec3 GetSleepingSpot(Building_Bed bed)
        {
            for (var i = 0; i < bed.SleepingSlotsCount; i++)
            {
                if (bed.GetCurOccupant(i) == null)
                {
                    return bed.GetSleepingSlotPos(i);
                }
            }

            return bed.GetSleepingSlotPos(0);
        }

        private bool IsInOrByBed(Building_Bed b, Pawn p)
        {
            for (var i = 0; i < b.SleepingSlotsCount; i++)
            {
                if (b.GetSleepingSlotPos(i).InHorDistOf(p.Position, 1f))
                {
                    return true;
                }
            }

            return false;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(BedInd, 2, 0);
            yield return Toils_Goto.Goto(SlotInd, PathEndMode.OnCell);
            yield return new Toil
            {
                initAction = delegate { ticksLeftThisToil = 300; },
                tickAction = delegate
                {
                    if (IsInOrByBed(Bed, Partner))
                    {
                        ticksLeftThisToil = 0;
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            var layDown = new Toil();
            layDown.initAction = delegate
            {
                layDown.actor.pather.StopDead();
                var curDriver = layDown.actor.jobs.curDriver;
                //				curDriver.layingDown = 2;
                curDriver.asleep = false;
            };
            layDown.tickAction = delegate { actor.GainComfortFromCellIfPossible(); };
            yield return layDown;
            var loveToil = new Toil
            {
                initAction = delegate
                {
                    ticksLeftThisToil = 1200;
                    if (!LovePartnerRelationUtility.HasAnyLovePartner(actor))
                    {
                        return;
                    }

                    var existingLovePartner = LovePartnerRelationUtility.ExistingLovePartner(actor);
                    if (Partner == existingLovePartner)
                    {
                        return;
                    }

                    if (existingLovePartner.Dead)
                    {
                        return;
                    }

                    if (existingLovePartner.Map == actor.Map || Rand.Value < 0.25)
                    {
                        existingLovePartner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, actor);
                    }
                },
                tickAction = delegate
                {
                    if (ticksLeftThisToil % 100 == 0)
                    {
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.Heart);
                    }

                    if (ticksLeftThisToil % 100 == 0)
                    {
                        actor.needs.joy.GainJoy(0.005f, RRRMiscDefOf.Lewd);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            loveToil.AddFailCondition(() => Partner.Dead || ticksLeftThisToil > 100 && !IsInOrByBed(Bed, Partner));
            yield return loveToil;
            yield return new Toil
            {
                initAction = delegate
                {
                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.GotSomeLovin, Partner);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}