using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(LovePartnerRelationUtility), "LovePartnerRelationGenerationChance", null)]
public static class LovePartnerRelationUtility_LovePartnerRelationGenerationChance
{
    // CHANGE: Updated with new orientation options.
    public static bool Prefix(Pawn generated, Pawn other, PawnGenerationRequest request, bool ex,
        ref float __result)
    {
        if (generated.ageTracker.AgeBiologicalYearsFloat < 14f)
        {
            __result = 0f;
            return false;
        }

        if (other.ageTracker.AgeBiologicalYearsFloat < 14f)
        {
            __result = 0f;
            return false;
        }

        if (generated.gender == other.gender && other.story.traits.HasTrait(RRRTraitDefOf.Straight))
        {
            __result = 0f;
            return false;
        }

        if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay))
        {
            __result = 0f;
            return false;
        }

        var single = 1f;
        if (ex)
        {
            var num = 0;
            var directRelations = other.relations.DirectRelations;
            foreach (var directPawnRelation in directRelations)
            {
                if (LovePartnerRelationUtility.IsExLovePartnerRelation(directPawnRelation.def))
                {
                    num++;
                }
            }

            single = Mathf.Pow(0.2f, num);
        }
        else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
        {
            __result = 0f;
            return false;
        }

        var single1 = generated.gender != other.gender ? 1f : 0.01f;
        var generationChanceAgeFactor = GetGenerationChanceAgeFactor(generated);
        var generationChanceAgeFactor1 = GetGenerationChanceAgeFactor(other);
        var generationChanceAgeGapFactor = GetGenerationChanceAgeGapFactor(generated, other, ex);
        var single2 = 1f;
        if (generated.GetRelations(other).Any(x => x.familyByBloodRelation))
        {
            single2 = 0.01f;
        }

        __result = single * generationChanceAgeFactor * generationChanceAgeFactor1 * generationChanceAgeGapFactor *
                   single1 * single2;
        return false;
    }

    private static float GetGenerationChanceAgeFactor(Pawn p)
    {
        var single = GenMath.LerpDouble(14f, 27f, 0f, 1f, p.ageTracker.AgeBiologicalYearsFloat);
        return Mathf.Clamp(single, 0f, 1f);
    }

    private static float GetGenerationChanceAgeGapFactor(Pawn p1, Pawn p2, bool ex)
    {
        var single = Mathf.Abs(p1.ageTracker.AgeBiologicalYearsFloat - p2.ageTracker.AgeBiologicalYearsFloat);
        if (ex)
        {
            var generateAsLovers = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p1, p2);
            if (generateAsLovers >= 0f)
            {
                single = Mathf.Min(single, generateAsLovers);
            }

            var generateAsLovers1 = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p2, p1);
            if (generateAsLovers1 >= 0f)
            {
                single = Mathf.Min(single, generateAsLovers1);
            }
        }

        if (single > 40f)
        {
            return 0f;
        }

        var single1 = GenMath.LerpDouble(0f, 20f, 1f, 0.001f, single);
        return Mathf.Clamp(single1, 0.001f, 1f);
    }

    private static float MinPossibleAgeGapAtMinAgeToGenerateAsLovers(Pawn p1, Pawn p2)
    {
        var ageChronologicalYearsFloat = p1.ageTracker.AgeChronologicalYearsFloat - 14f;
        if (ageChronologicalYearsFloat < 0f)
        {
            Log.Warning("at < 0");
            return 0f;
        }

        var single = PawnRelationUtility.MaxPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat,
            p2.ageTracker.AgeChronologicalYearsFloat, ageChronologicalYearsFloat);
        var single1 =
            PawnRelationUtility.MinPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat,
                ageChronologicalYearsFloat);
        switch (single)
        {
            case < 0f:
            case < 14f:
                return -1f;
        }

        if (single1 <= 14f)
        {
            return 0f;
        }

        return single1 - 14f;
    }
}