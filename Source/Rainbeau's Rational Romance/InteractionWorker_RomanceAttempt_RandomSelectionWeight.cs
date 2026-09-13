using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight),
    null)]
public static class InteractionWorker_RomanceAttempt_RandomSelectionWeight
{
    // CHANGE: Updated with new orientation options and traits.
    // CHANGE: Women are no less likely than men to initiate romance, when from colonist or glitterworld cultures.
    // CHANGE: Women are more forward when from tribal or imperial; men when from medieval or urbworld cultures. 
    // CHANGE: Pawn in mental break or who is already lover of initiator can't be targeted.
    // CHANGE: Pawn can't perform romance attempt if recently rebuffed.
    // CHANGE: Pawn can't target others or be targeted if current lover is good enough.
    // CHANGE: Allowed for polyamory.
    public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result)
    {
        EnsureOrientationAssigned(initiator);
        EnsureOrientationAssigned(recipient);

        if (ShouldBlockAttempt(initiator, recipient, out var romanceChance, out var opinionOfTarget))
        {
            __result = 0f;
            return false;
        }

        var cheatChance = CalculateCheatChance(initiator);
        var genderAggressiveness = GetGenderAggressiveness(initiator);
        var orientationMatch = GetOrientationMatch(initiator, recipient, ref genderAggressiveness);

        var romanceChancePercent = Mathf.InverseLerp(0.25f, 1f, romanceChance);
        var opinionPercent = Mathf.InverseLerp(5f, 100f, opinionOfTarget);

        __result = 1.15f * genderAggressiveness * romanceChancePercent * opinionPercent * cheatChance *
                   orientationMatch;
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

    private static bool ShouldBlockAttempt(Pawn initiator, Pawn recipient, out float romanceChance,
        out int opinionOfTarget)
    {
        romanceChance = 0f;
        opinionOfTarget = 0;

        if (recipient.InMentalState ||
            LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient) ||
            initiator.needs.mood.thoughts.memories.NumMemoriesOfDef(ThoughtDefOf.RebuffedMyRomanceAttempt) > 0)
        {
            return true;
        }

        romanceChance = initiator.relations.SecondaryRomanceChanceFactor(recipient);
        if (romanceChance < 0.25f)
        {
            return true;
        }

        opinionOfTarget = initiator.relations.OpinionOf(recipient);
        if (opinionOfTarget < 5 || recipient.relations.OpinionOf(initiator) < 5)
        {
            return true;
        }

        return HasCommittedPartnerBlock(initiator) || HasCommittedPartnerBlock(recipient);
    }

    private static bool HasCommittedPartnerBlock(Pawn pawn)
    {
        var currentPartner = LovePartnerRelationUtility.ExistingMostLikedLovePartner(pawn, false);
        return currentPartner != null &&
               pawn.relations.OpinionOf(currentPartner) >= 33 &&
               !SexualityUtilities.HasFreeLoverCapacity(pawn) &&
               !pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) &&
               !pawn.story.traits.HasTrait(RRRTraitDefOf.Philanderer);
    }

    private static float CalculateCheatChance(Pawn initiator)
    {
        var partner = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, false);
        if (partner == null || SexualityUtilities.HasFreeLoverCapacity(initiator))
        {
            return 1f;
        }

        if (initiator.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) &&
            !SexualityUtilities.HasNonPolyPartner(initiator))
        {
            return 1f;
        }

        var opinionOfPartner = initiator.relations.OpinionOf(partner);
        var cheatChance = initiator.story.traits.HasTrait(RRRTraitDefOf.Philanderer)
            ? GetPhilandererCheatChance(initiator, partner, opinionOfPartner)
            : Mathf.InverseLerp(50f, -50f, opinionOfPartner);

        if (initiator.story.traits.HasTrait(RRRTraitDefOf.Faithful))
        {
            cheatChance = 0f;
        }

        return cheatChance;
    }

    private static float GetPhilandererCheatChance(Pawn initiator, Pawn partner, float opinionOfPartner)
    {
        return initiator.Map == partner.Map
            ? Mathf.InverseLerp(75f, 5f, opinionOfPartner)
            : Mathf.InverseLerp(100f, 50f, opinionOfPartner);
    }

    private static float GetGenderAggressiveness(Pawn initiator)
    {
        var backgroundCulture = SexualityUtilities.GetAdultCulturalAdjective(initiator);
        return backgroundCulture switch
        {
            "Urbworld" => initiator.gender != Gender.Male ? 0.5f : 1f,
            "Imperial" => initiator.gender != Gender.Female ? 0.5f : 1f,
            "Tribal" => initiator.gender != Gender.Female ? 0.125f : 1f,
            "Medieval" => initiator.gender != Gender.Male ? 0.125f : 1f,
            _ => 1f
        };
    }

    private static float GetOrientationMatch(Pawn initiator, Pawn recipient, ref float genderAggressiveness)
    {
        var orientationMatch = 1f;
        if (initiator.story.traits.HasTrait(TraitDefOf.Asexual) ||
            recipient.story.traits.HasTrait(TraitDefOf.Asexual))
        {
            orientationMatch = 0.15f;
        }

        if (initiator.gender != recipient.gender &&
            (initiator.story.traits.HasTrait(TraitDefOf.Gay) || recipient.story.traits.HasTrait(TraitDefOf.Gay)))
        {
            orientationMatch = 0.15f;
        }

        if (initiator.gender != recipient.gender)
        {
            return orientationMatch;
        }

        genderAggressiveness = 1f;
        if (initiator.story.traits.HasTrait(RRRTraitDefOf.Straight) ||
            recipient.story.traits.HasTrait(RRRTraitDefOf.Straight))
        {
            orientationMatch = 0.15f;
        }

        return orientationMatch;
    }
}