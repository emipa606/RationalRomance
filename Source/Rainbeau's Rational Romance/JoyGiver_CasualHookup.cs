using RimWorld;
using Verse;
using Verse.AI;

namespace RationalRomance_Code;

public class JoyGiver_CasualHookup : JoyGiver
{
    public static readonly float percentRate = RationalRomance.Settings.hookupRate / 2;

    public override Job TryGiveJob(Pawn pawn)
    {
        Job result;
        if (!InteractionUtility.CanInitiateInteraction(pawn))
        {
            result = null;
        }
        else if (!SexualityUtilities.WillPawnTryHookup(pawn))
        {
            result = null;
        }
        else if (PawnUtility.WillSoonHaveBasicNeed(pawn))
        {
            result = null;
        }
        else
        {
            var pawn2 = SexualityUtilities.FindAttractivePawn(pawn);
            if (pawn2 == null)
            {
                result = null;
            }
            else
            {
                var bed = SexualityUtilities.FindHookupBed(pawn, pawn2);
                if (bed == null)
                {
                    result = null;
                }
                else if (100f * Rand.Value > percentRate)
                {
                    result = null;
                }
                else
                {
                    result = new Job(def.jobDef, pawn2, bed);
                }
            }
        }

        return result;
    }
}