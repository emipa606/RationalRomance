using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor), null)]
public static class Pawn_RelationsTracker_SecondaryLovinChanceFactor
{
    // CHANGE: Updated with new orientation options.
    // CHANGE: Gender age preferences are now the same, except for mild cultural variation.
    // CHANGE: Pawns with Ugly trait are less uninterested romantically in other ugly pawns.
    private static FieldInfo pawnFieldInfo;
    private static readonly TraitDef beauty = TraitDef.Named("Beauty");

    public static bool Prefix(Pawn otherPawn, ref float __result, ref Pawn_RelationsTracker __instance)
    {
        var pawn = __instance.getPawn();
        if (IsInvalidPair(pawn, otherPawn) || FailsOrientationGate(pawn, otherPawn))
        {
            __result = 0f;
            return false;
        }

        var targetAge = otherPawn.ageTracker.AgeBiologicalYearsFloat;
        if (targetAge < 16f)
        {
            __result = 0f;
            return false;
        }

        var ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
        var targetAgeLikelihood = CalculateTargetAgeLikelihood(ageBiologicalYearsFloat, targetAge);
        var targetBaseCapabilities = CalculateTargetBaseCapabilities(otherPawn);
        var targetBeautyMod = CalculateTargetBeautyModifier(pawn, otherPawn);
        var ageDiffPref = GetAgeDifferencePreference(pawn, otherPawn, ageBiologicalYearsFloat, targetAge);
        var crossSpecies = pawn.def != otherPawn.def ? RationalRomance.Settings.AlienLoveChance / 100f : 1f;

        var initiatorYoung = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
        var targetYoung = Mathf.InverseLerp(15f, 18f, targetAge);
        __result = targetAgeLikelihood * ageDiffPref * targetBaseCapabilities * initiatorYoung * targetYoung *
                   targetBeautyMod * crossSpecies;
        return false;
    }

    private static bool IsInvalidPair(Pawn pawn, Pawn otherPawn)
    {
        return pawn == otherPawn ||
               (!pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike) && pawn.def != otherPawn.def;
    }

    private static bool FailsOrientationGate(Pawn pawn, Pawn otherPawn)
    {
        if (Rand.ValueSeeded(pawn.thingIDNumber ^ 3273711) < 0.015f || !pawn.RaceProps.Humanlike)
        {
            return false;
        }

        return pawn.story.traits.HasTrait(TraitDefOf.Asexual) ||
               pawn.story.traits.HasTrait(TraitDefOf.Gay) && otherPawn.gender != pawn.gender ||
               pawn.story.traits.HasTrait(RRRTraitDefOf.Straight) && otherPawn.gender == pawn.gender;
    }

    private static float CalculateTargetAgeLikelihood(float initiatorAge, float targetAge)
    {
        var youngestTargetAge = Mathf.Max(16f, initiatorAge - 30f);
        var youngestReasonableTargetAge = Mathf.Max(20f, initiatorAge, initiatorAge - 10f);
        return GenMath.FlatHill(0.15f, youngestTargetAge, youngestReasonableTargetAge, initiatorAge + 7f,
            initiatorAge + 30f, 0.15f, targetAge);
    }

    private static float CalculateTargetBaseCapabilities(Pawn otherPawn)
    {
        var capabilityMultiplier = 1f;
        capabilityMultiplier *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
        capabilityMultiplier *=
            Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
        capabilityMultiplier *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
        return capabilityMultiplier;
    }

    private static float CalculateTargetBeautyModifier(Pawn pawn, Pawn otherPawn)
    {
        var initiatorBeauty = 0;
        var targetBeauty = 0;
        if (otherPawn.RaceProps.Humanlike)
        {
            initiatorBeauty = pawn.story.traits.DegreeOfTrait(beauty);
            targetBeauty = otherPawn.story.traits.DegreeOfTrait(beauty);
        }

        return targetBeauty switch
        {
            -2 => initiatorBeauty >= 0 ? 0.3f : 0.8f,
            -1 => initiatorBeauty >= 0 ? 0.75f : 0.9f,
            1 => 1.7f,
            2 => 2.3f,
            _ => 1f
        };
    }

    private static float GetAgeDifferencePreference(Pawn pawn, Pawn otherPawn, float initiatorAge, float targetAge)
    {
        var backgroundCulture = SexualityUtilities.GetAdultCulturalAdjective(pawn);
        var initiatorYounger = initiatorAge <= targetAge;

        return backgroundCulture switch
        {
            "Urbworld" or "Medieval" => GetGenderAgePreference(pawn.gender, otherPawn.gender, initiatorYounger, true),
            "Tribal" or "Imperial" => GetGenderAgePreference(pawn.gender, otherPawn.gender, initiatorYounger, false),
            _ => 1f
        };
    }

    private static float GetGenderAgePreference(Gender initiatorGender, Gender targetGender, bool initiatorYounger,
        bool malePrefersYounger)
    {
        if (initiatorGender == Gender.Male && targetGender == Gender.Female)
        {
            return malePrefersYounger
                ? initiatorYounger ? 0.8f : 1.2f
                : initiatorYounger
                    ? 1.2f
                    : 0.8f;
        }

        if (initiatorGender == Gender.Female && targetGender == Gender.Male)
        {
            return malePrefersYounger
                ? initiatorYounger ? 1.2f : 0.8f
                : initiatorYounger
                    ? 0.8f
                    : 1.2f;
        }

        return 1f;
    }

    private static Pawn getPawn(this Pawn_RelationsTracker _this)
    {
        if (pawnFieldInfo != null)
        {
            return (Pawn)pawnFieldInfo.GetValue(_this);
        }

        pawnFieldInfo = typeof(Pawn_RelationsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
        if (pawnFieldInfo == null)
        {
            Log.ErrorOnce("Unable to reflect Pawn_RelationsTracker.pawn!", 305432421);
        }

        return (Pawn)pawnFieldInfo?.GetValue(_this);
    }
}