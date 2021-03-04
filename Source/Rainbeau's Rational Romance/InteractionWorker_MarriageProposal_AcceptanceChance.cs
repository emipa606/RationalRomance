using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code
{
    [HarmonyPatch(typeof(InteractionWorker_MarriageProposal), "AcceptanceChance", null)]
    public static class InteractionWorker_MarriageProposal_AcceptanceChance
    {
        // CHANGE: Pawns will always reject marriage proposals if proposer is of non-ideal gender.
        public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (!recipient.story.traits.HasTrait(TraitDefOf.Asexual) &&
                !recipient.story.traits.HasTrait(TraitDefOf.Bisexual) &&
                !recipient.story.traits.HasTrait(TraitDefOf.Gay) &&
                !recipient.story.traits.HasTrait(RRRTraitDefOf.Straight))
            {
                ExtraTraits.AssignOrientation(recipient);
            }

            if (initiator.gender == recipient.gender && recipient.story.traits.HasTrait(RRRTraitDefOf.Straight))
            {
                __result = 0f;
                return false;
            }

            if (initiator.gender != recipient.gender && recipient.story.traits.HasTrait(TraitDefOf.Gay))
            {
                __result = 0f;
                return false;
            }

            if (recipient.story.traits.HasTrait(TraitDefOf.Asexual))
            {
                __result = 0f;
                return false;
            }

            var single = 0.9f;
            single *= Mathf.Clamp01(GenMath.LerpDouble(-20f, 60f, 0f, 1f, recipient.relations.OpinionOf(initiator)));
            __result = Mathf.Clamp01(single);
            return false;
        }
    }
}