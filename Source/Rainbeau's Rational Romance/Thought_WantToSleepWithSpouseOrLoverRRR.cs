using RimWorld;
using Verse;

namespace RationalRomance_Code
{
    public class Thought_WantToSleepWithSpouseOrLoverRRR : Thought_WantToSleepWithSpouseOrLover
    {
        public override string LabelCap
        {
            get
            {
                if (pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous))
                {
                    return string.Format(CurStage.label, "my partners").CapitalizeFirst();
                }

                var directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, false);
                return string.Format(CurStage.label, directPawnRelation.otherPawn.LabelShort).CapitalizeFirst();
            }
        }
    }
}