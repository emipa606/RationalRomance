using RimWorld;
using Verse;

namespace RationalRomance_Code;

public static class ExtraTraits
{
    public static void AssignOrientation(Pawn pawn)
    {
        var orientation = Rand.Value;
        if (pawn.gender == Gender.None)
        {
            return;
        }

        if (orientation < RationalRomance.Settings.AsexualChance / 100)
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
        else if (orientation < (RationalRomance.Settings.AsexualChance + RationalRomance.Settings.BisexualChance) /
                 100)
        {
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Bisexual));
        }
        else if (orientation < (RationalRomance.Settings.AsexualChance + RationalRomance.Settings.BisexualChance +
                                RationalRomance.Settings.GayChance) / 100)
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

        if (Rand.Value < RationalRomance.Settings.PolyChance / 100)
        {
            pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Polyamorous));
        }
    }
}