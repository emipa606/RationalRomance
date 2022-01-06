using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), "RandomSelectionWeight", null)]
public static class InteractionWorker_MarriageProposal_RandomSelectionWeight
{
    // CHANGE: Female pawns are now just as likely to propose as male pawns, with cultural variations.
    // CHANGE: Marriage won't be proposed to someone of non-ideal gender.
    public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
    {
        if (!initiator.story.traits.HasTrait(TraitDefOf.Asexual) &&
            !initiator.story.traits.HasTrait(TraitDefOf.Bisexual) &&
            !initiator.story.traits.HasTrait(TraitDefOf.Gay) &&
            !initiator.story.traits.HasTrait(RRRTraitDefOf.Straight))
        {
            ExtraTraits.AssignOrientation(initiator);
        }

        var directRelation = initiator.relations.GetDirectRelation(PawnRelationDefOf.Lover, recipient);
        if (directRelation == null)
        {
            __result = 0f;
            return false;
        }

        if (!SexualityUtilities.HasFreeSpouseCapacity(initiator) || !SexualityUtilities.HasFreeSpouseCapacity(recipient))
        {
            __result = 0f;
            return false;
        }

        if (initiator.gender == recipient.gender && initiator.story.traits.HasTrait(RRRTraitDefOf.Straight))
        {
            __result = 0f;
            return false;
        }

        if (initiator.gender != recipient.gender && initiator.story.traits.HasTrait(TraitDefOf.Gay))
        {
            __result = 0f;
            return false;
        }

        if (initiator.story.traits.HasTrait(TraitDefOf.Asexual))
        {
            __result = 0f;
            return false;
        }

        float genderAggressiveness;
        var backgroundCulture = SexualityUtilities.GetAdultCulturalAdjective(initiator);
        if (backgroundCulture == "Urbworld")
        {
            genderAggressiveness = initiator.gender != Gender.Male ? 0.75f : 1f;
        }
        else if (backgroundCulture == "Imperial")
        {
            genderAggressiveness = initiator.gender != Gender.Female ? 0.75f : 1f;
        }
        else if (backgroundCulture == "Tribal")
        {
            genderAggressiveness = initiator.gender != Gender.Female ? 0.2f : 1f;
        }
        else if (backgroundCulture == "Medieval")
        {
            genderAggressiveness = initiator.gender != Gender.Male ? 0.2f : 1f;
        }
        else
        {
            genderAggressiveness = 1f;
        }

        var single = 0.4f;
        var ticksGame = Find.TickManager.TicksGame;
        var single1 = (ticksGame - directRelation.startTicks) / 60000f;
        single *= Mathf.InverseLerp(0f, 60f, single1);
        single *= Mathf.InverseLerp(0f, 60f, initiator.relations.OpinionOf(recipient));
        if (recipient.relations.OpinionOf(initiator) < 0)
        {
            single *= 0.3f;
        }

        var psylove = SexualityUtilities.IsPsychicLoveActive(initiator, recipient) ? 10f : 1f;
        __result = single * genderAggressiveness * psylove;
        return false;
    }
}