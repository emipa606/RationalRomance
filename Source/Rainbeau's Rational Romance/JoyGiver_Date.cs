using RimWorld;
using Verse;
using Verse.AI;

namespace RationalRomance_Code;

public class JoyGiver_Date : JoyGiver
{
    private static readonly float percentRate = RationalRomance.Settings.DateRate / 2;

    public override Job TryGiveJob(Pawn pawn)
    {
        Job result;
        if (!SocialInteractionUtility.CanInitiateInteraction(pawn))
        {
            result = null;
        }
        else
        {
            var pawn2 = LovePartnerRelationUtility.HasAnyLovePartner(pawn)
                ? LovePartnerRelationUtility.ExistingLovePartners(pawn).RandomElement().otherPawn
                : SexualityUtilities.FindAttractivePawn(pawn);

            if (pawn2 is not { Spawned: true } || !pawn2.Awake() || !JoyUtility.EnjoyableOutsideNow(pawn) ||
                PawnUtility.WillSoonHaveBasicNeed(pawn) || 100f * Rand.Value > percentRate)
            {
                result = null;
            }
            else
            {
                result = new Job(def.jobDef, pawn2);
            }
        }

        return result;
    }
}