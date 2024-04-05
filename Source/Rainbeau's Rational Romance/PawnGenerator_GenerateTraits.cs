using HarmonyLib;
using RimWorld;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GenerateTraits), null)]
public static class PawnGenerator_GenerateTraits
{
    // CHANGE: Add orientation trait after other traits are selected.
    public static void Postfix(Pawn pawn)
    {
        if (pawn.story.traits.HasTrait(TraitDefOf.Asexual) || pawn.story.traits.HasTrait(TraitDefOf.Bisexual) ||
            pawn.story.traits.HasTrait(TraitDefOf.Gay) || pawn.story.traits.HasTrait(RRRTraitDefOf.Straight))
        {
            return;
        }

        ExtraTraits.AssignOrientation(pawn);
    }
}