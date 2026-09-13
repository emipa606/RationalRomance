using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.SuccessChance), null)]
public static class InteractionWorker_RomanceAttempt_SuccessChance
{
    // CHANGE: Updated with new orientation options and traits.
    // CHANGE: Pawns are more likely to rebuff non-ideal partners.
    // CHANGE: Allowed for polyamory.
    public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
    {
        if (InteractionWorker_RomanceAttempt.CanCreatePsychicBondBetween(initiator, recipient))
        {
            if (initiator.IsQuestHelper() || recipient.IsQuestHelper())
            {
                __result = 0f;
                return false;
            }

            __result = 1f;
            return false;
        }

        EnsureOrientationAssigned(recipient);

        var successChance = CalculateBaseSuccessChance(initiator, recipient);
        if (ShouldApplyCommitmentPenalty(recipient))
        {
            successChance *= CalculateCommitmentModifier(recipient);
        }

        __result = ApplyOrientationModifiers(initiator, recipient, Mathf.Clamp01(successChance));
        return false;
    }

    private static void EnsureOrientationAssigned(Pawn pawn)
    {
        if (pawn.story.traits.HasTrait(TraitDefOf.Asexual) ||
            pawn.story.traits.HasTrait(TraitDefOf.Bisexual) ||
            pawn.story.traits.HasTrait(TraitDefOf.Gay) ||
            pawn.story.traits.HasTrait(RRRTraitDefOf.Straight))
        {
            return;
        }

        ExtraTraits.AssignOrientation(pawn);
    }

    private static float CalculateBaseSuccessChance(Pawn initiator, Pawn recipient)
    {
        var successChance = 0.6f;
        successChance *= recipient.relations.SecondaryRomanceChanceFactor(initiator);
        successChance *= Mathf.InverseLerp(5f, 100f, recipient.relations.OpinionOf(initiator));
        return successChance;
    }

    private static bool ShouldApplyCommitmentPenalty(Pawn recipient)
    {
        if (SexualityUtilities.HasFreeLoverCapacity(recipient))
        {
            return false;
        }

        return !recipient.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) ||
               SexualityUtilities.HasNonPolyPartner(recipient);
    }

    private static float CalculateCommitmentModifier(Pawn recipient)
    {
        var commitmentMultiplier = GetCommitmentMultiplier(recipient, out var currentPartner);
        if (currentPartner == null)
        {
            return commitmentMultiplier;
        }

        commitmentMultiplier *= Mathf.InverseLerp(100f, 0f, recipient.relations.OpinionOf(currentPartner));
        if (recipient.story.traits.HasTrait(RRRTraitDefOf.Philanderer))
        {
            commitmentMultiplier *= 1.6f;
            if (currentPartner.Map != recipient.Map)
            {
                commitmentMultiplier *= 2f;
            }
        }

        commitmentMultiplier *= Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(currentPartner));
        return commitmentMultiplier;
    }

    private static float GetCommitmentMultiplier(Pawn recipient, out Pawn currentPartner)
    {
        currentPartner = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, x => !x.Dead);
        if (currentPartner != null)
        {
            return recipient.story.traits.HasTrait(RRRTraitDefOf.Faithful) ? 0f : 0.6f;
        }

        currentPartner = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, x => !x.Dead);
        if (currentPartner != null)
        {
            return recipient.story.traits.HasTrait(RRRTraitDefOf.Faithful) ? 0f : 0.1f;
        }

        if (recipient.GetSpouseCount(false) <= 0)
        {
            return 1f;
        }

        // This is a change from vanilla that checks for MostLikedSpouseRelation, but least liked seems better.
        currentPartner = recipient.GetLeastLikedSpouseRelation().otherPawn;
        return recipient.story.traits.HasTrait(RRRTraitDefOf.Faithful) ? 0f : 0.3f;
    }

    private static float ApplyOrientationModifiers(Pawn initiator, Pawn recipient, float successChance)
    {
        if (initiator.gender == recipient.gender && recipient.story.traits.HasTrait(RRRTraitDefOf.Straight))
        {
            successChance *= 0.6f;
        }

        if (initiator.gender != recipient.gender && recipient.story.traits.HasTrait(TraitDefOf.Gay))
        {
            successChance *= 0.6f;
        }

        if (recipient.story.traits.HasTrait(TraitDefOf.Asexual))
        {
            successChance *= 0.3f;
        }

        return successChance;
    }
}