using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(InteractionWorker_Breakup), "RandomSelectionWeight", null)]
public static class InteractionWorker_Breakup_RandomSelectionWeight
{
    // CHANGE: Pawns are more likely to break up if currently with non-ideal partner.
    public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
    {
        if (!initiator.story.traits.HasTrait(TraitDefOf.Asexual) &&
            !initiator.story.traits.HasTrait(TraitDefOf.Bisexual) &&
            !initiator.story.traits.HasTrait(TraitDefOf.Gay) &&
            !initiator.story.traits.HasTrait(RRRTraitDefOf.Straight))
        {
            ExtraTraits.AssignOrientation(initiator);
        }

        if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
        {
            __result = 0f;
            return false;
        }

        var single = Mathf.InverseLerp(100f, -100f, initiator.relations.OpinionOf(recipient));
        var single1 = 1f;
        if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
        {
            single1 = 0.4f;
        }

        var single2 = 1f;

        if (SexualityUtilities.IsPsychicLoveActive(initiator, recipient))
        {
            single2 = 0.1f;
        }

        __result = 0.02f * single * single1 * single2;
        if (initiator.gender == recipient.gender && initiator.story.traits.HasTrait(RRRTraitDefOf.Straight))
        {
            __result *= 2f;
        }

        if (initiator.gender != recipient.gender && initiator.story.traits.HasTrait(TraitDefOf.Gay))
        {
            __result *= 2f;
        }

        if (initiator.story.traits.HasTrait(TraitDefOf.Asexual))
        {
            __result *= 2f;
        }

        return false;
    }
}