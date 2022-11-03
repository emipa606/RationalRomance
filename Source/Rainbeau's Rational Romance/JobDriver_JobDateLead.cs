using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace RationalRomance_Code;

public class JobDriver_JobDateLead : JobDriver
{
    private const int duration = 20;
    private readonly TargetIndex PartnerInd = TargetIndex.A;
    private int ticksLeft = 20;

    private Pawn actor => GetActor();

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

    public override RandomSocialMode DesiredSocialMode()
    {
        return RandomSocialMode.SuperActive;
    }

    //private bool IsPartnerNearby() {
    //	return this.actor.Position.InHorDistOf(this.Partner.Position, 2f);
    //}
    public Toil GotoCell(LocalTargetInfo target)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var toilActor = toil.actor;
            toilActor.pather.StartPath(target, PathEndMode.OnCell);
        };
        toil.tickAction = delegate { actor.needs.joy.GainJoy(0.0001f, RRRMiscDefOf.Social); };
        toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        toil.AddFailCondition(() => !Partner.Spawned);
        toil.AddFailCondition(() => Partner.Dead);
        toil.AddFailCondition(() => Partner.CurJob.def != RRRJobDefOf.JobDateFollow);
        return toil;
    }

    private Toil WaitForPartner()
    {
        var toil = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            initAction = delegate { ticksLeftThisToil = 700; },
            tickAction = delegate { actor.needs.joy.GainJoy(0.0001f, RRRMiscDefOf.Social); }
        };
        toil.AddFailCondition(() =>
            PawnUtility.WillSoonHaveBasicNeed(actor) || PawnUtility.WillSoonHaveBasicNeed(Partner));
        toil.AddFailCondition(() => !Partner.Spawned);
        toil.AddFailCondition(() => Partner.Dead);
        toil.AddFailCondition(() => Partner.CurJob.def != RRRJobDefOf.JobDateFollow);
        return toil;
    }

    [DebuggerHidden]
    public override IEnumerable<Toil> MakeNewToils()
    {
        foreach (var target in job.targetQueueB)
        {
            yield return GotoCell(target.Cell);
            yield return WaitForPartner();
        }
    }
}