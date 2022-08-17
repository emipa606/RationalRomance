using HarmonyLib;
using RimWorld;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(LovePartnerRelationUtility), "ChangeSpouseRelationsToExSpouse", null)]
public static class LovePartnerRelationUtility_ChangeSpouseRelationsToExSpouse
{
    // CHANGE: Allowed for polyamory.
    public static bool Prefix(Pawn pawn)
    {
        if (!pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous))
        {
            return true;
        }

        var spouses = pawn.GetSpouses(true);
        foreach (var spousePawn in spouses)
        {
            if (!spousePawn.Dead && (spousePawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) ||
                                     SexualityUtilities.HasFreeLoverCapacity(pawn)))
            {
                continue;
            }

            pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, spousePawn);
            pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, spousePawn);
        }

        return false;
    }
}