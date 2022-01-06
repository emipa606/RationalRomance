using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryLovinChanceFactor", null)]
public static class Pawn_RelationsTracker_SecondaryLovinChanceFactor
{
    // CHANGE: Updated with new orientation options.
    // CHANGE: Gender age preferences are now the same, except for mild cultural variation.
    // CHANGE: Pawns with Ugly trait are less uninterested romantically in other ugly pawns.
    internal static FieldInfo _pawn;

    public static bool Prefix(Pawn otherPawn, ref float __result, ref Pawn_RelationsTracker __instance)
    {
        var pawn = __instance.GetPawn();
        if (pawn == otherPawn)
        {
            __result = 0f;
            return false;
        }

        if ((!pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike) && pawn.def != otherPawn.def)
        {
            __result = 0f;
            return false;
        }

        float crossSpecies = 1;
        if (pawn.def != otherPawn.def)
        {
            crossSpecies = RationalRomance.Settings.alienLoveChance / 100;
        }

        if (Rand.ValueSeeded(pawn.thingIDNumber ^ 3273711) >= 0.015f)
        {
            if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Asexual))
            {
                __result = 0f;
                return false;
            }

            if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
            {
                if (otherPawn.gender != pawn.gender)
                {
                    __result = 0f;
                    return false;
                }
            }

            if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(RRRTraitDefOf.Straight))
            {
                if (otherPawn.gender == pawn.gender)
                {
                    __result = 0f;
                    return false;
                }
            }
        }

        var ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
        var targetAge = otherPawn.ageTracker.AgeBiologicalYearsFloat;
        if (targetAge < 16f)
        {
            __result = 0f;
            return false;
        }

        var youngestTargetAge = Mathf.Max(16f, ageBiologicalYearsFloat - 30f);
        var youngestReasonableTargetAge = Mathf.Max(20f, ageBiologicalYearsFloat, ageBiologicalYearsFloat - 10f);
        var targetAgeLikelihood = GenMath.FlatHill(0.15f, youngestTargetAge, youngestReasonableTargetAge,
            ageBiologicalYearsFloat + 7f, ageBiologicalYearsFloat + 30f, 0.15f, targetAge);
        var targetBaseCapabilities = 1f;
        targetBaseCapabilities *=
            Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
        targetBaseCapabilities *=
            Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
        targetBaseCapabilities *=
            Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
        var initiatorBeauty = 0;
        var targetBeauty = 0;
        if (otherPawn.RaceProps.Humanlike)
        {
            initiatorBeauty = pawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
        }

        if (otherPawn.RaceProps.Humanlike)
        {
            targetBeauty = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
        }

        var targetBeautyMod = 1f;
        if (targetBeauty == -2)
        {
            targetBeautyMod = initiatorBeauty >= 0 ? 0.3f : 0.8f;
        }

        if (targetBeauty == -1)
        {
            targetBeautyMod = initiatorBeauty >= 0 ? 0.75f : 0.9f;
        }

        if (targetBeauty == 1)
        {
            targetBeautyMod = 1.7f;
        }
        else if (targetBeauty == 2)
        {
            targetBeautyMod = 2.3f;
        }

        var backgroundCulture = SexualityUtilities.GetAdultCulturalAdjective(pawn);
        var ageDiffPref = 1f;
        if (backgroundCulture == "Urbworld" || backgroundCulture == "Medieval")
        {
            if (pawn.gender == Gender.Male && otherPawn.gender == Gender.Female)
            {
                ageDiffPref = ageBiologicalYearsFloat <= targetAge ? 0.8f : 1.2f;
            }
            else if (pawn.gender == Gender.Female && otherPawn.gender == Gender.Male)
            {
                ageDiffPref = ageBiologicalYearsFloat <= targetAge ? 1.2f : 0.8f;
            }
        }

        if (backgroundCulture == "Tribal" || backgroundCulture == "Imperial")
        {
            if (pawn.gender == Gender.Male && otherPawn.gender == Gender.Female)
            {
                ageDiffPref = ageBiologicalYearsFloat <= targetAge ? 1.2f : 0.8f;
            }
            else if (pawn.gender == Gender.Female && otherPawn.gender == Gender.Male)
            {
                ageDiffPref = ageBiologicalYearsFloat <= targetAge ? 0.8f : 1.2f;
            }
        }

        var initiatorYoung = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
        var targetYoung = Mathf.InverseLerp(15f, 18f, targetAge);
        __result = targetAgeLikelihood * ageDiffPref * targetBaseCapabilities * initiatorYoung * targetYoung *
                   targetBeautyMod * crossSpecies;
        return false;
    }

    private static Pawn GetPawn(this Pawn_RelationsTracker _this)
    {
        if (!(_pawn == null))
        {
            return (Pawn)_pawn.GetValue(_this);
        }

        _pawn = typeof(Pawn_RelationsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
        if (_pawn == null)
        {
            Log.ErrorOnce("Unable to reflect Pawn_RelationsTracker.pawn!", 305432421);
        }

        return (Pawn)_pawn?.GetValue(_this);
    }
}