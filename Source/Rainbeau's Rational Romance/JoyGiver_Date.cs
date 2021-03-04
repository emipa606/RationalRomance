using RimWorld;
using Verse;
using Verse.AI;

namespace RationalRomance_Code
{
    public class JoyGiver_Date : JoyGiver
    {
        public static float percentRate = RationalRomance.Settings.dateRate / 2;

        public override Job TryGiveJob(Pawn pawn)
        {
            Job result;
            if (!InteractionUtility.CanInitiateInteraction(pawn))
            {
                result = null;
            }
            else
            {
                Pawn pawn2;
                if (LovePartnerRelationUtility.HasAnyLovePartner(pawn))
                {
                    pawn2 = LovePartnerRelationUtility.ExistingLovePartner(pawn);
                }
                else
                {
                    // allow pawns to choose any attractive pawn if they don't have a lover
                    pawn2 = SexualityUtilities.FindAttractivePawn(pawn);
                }

                if (pawn2 == null)
                {
                    result = null;
                }
                else if (!pawn2.Spawned)
                {
                    result = null;
                }
                else if (!pawn2.Awake())
                {
                    result = null;
                }
                else if (!JoyUtility.EnjoyableOutsideNow(pawn))
                {
                    result = null;
                }
                else if (PawnUtility.WillSoonHaveBasicNeed(pawn))
                {
                    result = null;
                }
                else if (100f * Rand.Value > percentRate)
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
}