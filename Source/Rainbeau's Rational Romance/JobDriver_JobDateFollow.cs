using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace RationalRomance_Code;

public class JobDriver_JobDateFollow : JobDriver
{
    private const TargetIndex PartnerInd = TargetIndex.A;

    private Pawn Actor => GetActor();

    private Pawn Partner => (Pawn)(Thing)job.GetTarget(PartnerInd);

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override RandomSocialMode DesiredSocialMode()
    {
        return RandomSocialMode.SuperActive;
    }

    //private bool IsPartnerNearby() {
    //	return this.actor.Position.InHorDistOf(this.Partner.Position, 2f);
    //}
    [DebuggerHidden]
    public override IEnumerable<Toil> MakeNewToils()
    {
        // Wait a tick to avoid a 1.1 issue where the date leader now must end their current
        // job after the date follower, causing the date follower to think the leader was no
        // longer leading the date and end this job.
        var waitForPartnerJob = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            initAction = delegate { ticksLeftThisToil = 1; }
        };
        yield return waitForPartnerJob;

        var followPartner = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Delay
        };
        followPartner.AddFailCondition(() => !Partner.Spawned);
        followPartner.AddFailCondition(() => Partner.Dead);
        followPartner.AddFailCondition(() => Partner.CurJob.def != RRRJobDefOf.JobDateLead);
        followPartner.initAction = delegate
        {
            ticksLeftThisToil = 200;
            Actor.pather.StartPath(Partner, PathEndMode.Touch);
        };
        followPartner.tickAction = delegate { Actor.needs.joy.GainJoy(0.0001f, RRRMiscDefOf.Social); };
        for (var i = 0; i < 100; i++)
        {
            yield return followPartner;
        }
    }
}