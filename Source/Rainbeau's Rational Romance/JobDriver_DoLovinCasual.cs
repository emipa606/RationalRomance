using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace RationalRomance_Code;

public class JobDriver_DoLovinCasual : JobDriver
{
    private const int TicksBetweenHeartMotes = 100;
    private const int Duration = 20;
    private const TargetIndex BedInd = TargetIndex.B;
    private const TargetIndex PartnerInd = TargetIndex.A;
    private const TargetIndex SlotInd = TargetIndex.C;
    private int ticksLeft = 20;

    private Building_Bed Bed => (Building_Bed)(Thing)job.GetTarget(BedInd);

    private Pawn Actor => GetActor();

    private Pawn Partner => (Pawn)(Thing)job.GetTarget(PartnerInd);

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft");
    }

    private static bool isInOrByBed(Building_Bed b, Pawn p)
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
    public override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Reserve.Reserve(BedInd, 2, 0);
        yield return Toils_Goto.Goto(SlotInd, PathEndMode.OnCell);
        yield return new Toil
        {
            initAction = delegate { ticksLeftThisToil = 300; },
            tickAction = delegate
            {
                if (isInOrByBed(Bed, Partner))
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
        layDown.tickIntervalAction = delegate(int delta) { Actor.GainComfortFromCellIfPossible(delta); };
        yield return layDown;
        var loveToil = new Toil
        {
            initAction = delegate
            {
                ticksLeftThisToil = 1200;
                if (!LovePartnerRelationUtility.HasAnyLovePartner(Actor))
                {
                    return;
                }

                var existingLovePartners = LovePartnerRelationUtility.ExistingLovePartners(Actor, false);
                if (existingLovePartners.Any(relation => relation.otherPawn == Partner))
                {
                    return;
                }

                foreach (var existingLovePartner in existingLovePartners)
                {
                    if (existingLovePartner.otherPawn.Map == Actor.Map || Rand.Value < 0.25)
                    {
                        existingLovePartner.otherPawn.needs.mood.thoughts.memories.TryGainMemory(
                            ThoughtDefOf.CheatedOnMe, Actor);
                    }
                }
            },
            tickAction = delegate
            {
                if (ticksLeftThisToil % TicksBetweenHeartMotes == 0)
                {
                    FleckMaker.ThrowMetaIcon(Actor.Position, Actor.Map, FleckDefOf.Heart);
                }

                if (ticksLeftThisToil % TicksBetweenHeartMotes == 0)
                {
                    Actor.needs.joy.GainJoy(0.005f, RRRMiscDefOf.Lewd);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Delay
        };
        loveToil.AddFailCondition(() =>
            Partner.Dead || ticksLeftThisToil > TicksBetweenHeartMotes && !isInOrByBed(Bed, Partner));
        yield return loveToil;
        yield return new Toil
        {
            initAction = delegate
            {
                Actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.GotSomeLovin, Partner);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}