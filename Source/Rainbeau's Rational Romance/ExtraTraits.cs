using RimWorld;
using Verse;

namespace RationalRomance_Code
{
    public static class ExtraTraits
    {
        public static void AssignOrientation(Pawn pawn)
        {
            var orientation = Rand.Value;
            if (pawn.gender == Gender.None)
            {
                return;
            }

            if (orientation < RationalRomance.Settings.asexualChance / 100)
            {
                if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) ||
                    LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
                {
                    pawn.story.traits.GainTrait(new Trait(TraitDefOf.Bisexual));
                }
                else if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) ||
                         LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn))
                {
                    pawn.story.traits.GainTrait(new Trait(TraitDefOf.Bisexual));
                }
                else if (pawn.story.traits.HasTrait(RRRTraitDefOf.Philanderer))
                {
                    pawn.story.traits.GainTrait(new Trait(TraitDefOf.Bisexual));
                }
                else
                {
                    pawn.story.traits.GainTrait(new Trait(TraitDefOf.Asexual));
                }
            }
            else if (orientation < (RationalRomance.Settings.asexualChance + RationalRomance.Settings.bisexualChance) /
                100)
            {
                pawn.story.traits.GainTrait(new Trait(TraitDefOf.Bisexual));
            }
            else if (orientation < (RationalRomance.Settings.asexualChance + RationalRomance.Settings.bisexualChance +
                                    RationalRomance.Settings.gayChance) / 100)
            {
                if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) ||
                    LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
                {
                    pawn.story.traits.GainTrait(new Trait(TraitDefOf.Bisexual));
                }
                else
                {
                    pawn.story.traits.GainTrait(new Trait(TraitDefOf.Gay));
                }
            }
            else
            {
                if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) ||
                    LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn))
                {
                    pawn.story.traits.GainTrait(new Trait(TraitDefOf.Bisexual));
                }
                else
                {
                    pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Straight));
                }
            }

            if (pawn.story.traits.HasTrait(TraitDefOf.Asexual) || pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous))
            {
                return;
            }

            if (Rand.Value < RationalRomance.Settings.polyChance / 100)
            {
                pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Polyamorous));
            }
        }
    }
}