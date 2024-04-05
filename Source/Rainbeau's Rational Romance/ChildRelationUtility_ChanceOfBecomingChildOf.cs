using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(ChildRelationUtility), "ChanceOfBecomingChildOf", null)]
public static class ChildRelationUtility_ChanceOfBecomingChildOf
{
    // CHANGE: Removed bias against gays being assigned as parents.
    public static bool Prefix(Pawn child, Pawn father, Pawn mother, PawnGenerationRequest? childGenerationRequest,
        PawnGenerationRequest? fatherGenerationRequest, PawnGenerationRequest? motherGenerationRequest,
        ref float __result)
    {
        if (father != null && father.gender != Gender.Male)
        {
            Log.Warning($"Tried to calculate chance for father with gender \"{father.gender}\".");
            __result = 0f;
            return false;
        }

        if (mother != null && mother.gender != Gender.Female)
        {
            Log.Warning($"Tried to calculate chance for mother with gender \"{mother.gender}\".");
            __result = 0f;
            return false;
        }

        if (father != null && child.GetFather() != null && child.GetFather() != father)
        {
            __result = 0f;
            return false;
        }

        if (mother != null && child.GetMother() != null && child.GetMother() != mother)
        {
            __result = 0f;
            return false;
        }

        if (mother != null && father != null &&
            !LovePartnerRelationUtility.LovePartnerRelationExists(mother, father) &&
            !LovePartnerRelationUtility.ExLovePartnerRelationExists(mother, father))
        {
            __result = 0f;
            return false;
        }

        var skinColorFactor = GetSkinColorFactor(child.story.melanin, father?.story.melanin, mother?.story.melanin);
        if (skinColorFactor <= 0f)
        {
            __result = 0f;
            return false;
        }

        var parentAgeFactor = 1f;
        var single = 1f;
        var childrenCount = 1f;
        var single1 = 1f;
        if (father != null && child.GetFather() == null)
        {
            parentAgeFactor = GetParentAgeFactor(father, child, 14f, 30f, 50f);
            if (parentAgeFactor == 0f)
            {
                __result = 0f;
                return false;
            }
        }

        if (mother != null && child.GetMother() == null)
        {
            single = GetParentAgeFactor(mother, child, 16f, 27f, 45f);
            if (single == 0f)
            {
                __result = 0f;
                return false;
            }

            var num = NumberOfChildrenFemaleWantsEver(mother);
            if (mother.relations.ChildrenCount >= num)
            {
                __result = 0f;
                return false;
            }

            childrenCount = 1f - (mother.relations.ChildrenCount / (float)num);
        }

        var single2 = 1f;
        var firstDirectRelationPawn = mother?.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
        if (firstDirectRelationPawn != null && firstDirectRelationPawn != father)
        {
            single2 *= 0.15f;
        }

        var pawn = father?.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
        if (pawn != null && pawn != mother)
        {
            single2 *= 0.15f;
        }

        __result = skinColorFactor * parentAgeFactor * single * childrenCount * single2 * single1;
        return false;
    }

    private static float GetSkinColorFactor(float? childMelanin, float? fatherMelanin, float? motherMelanin)
    {
        if (!childMelanin.HasValue || !fatherMelanin.HasValue || !motherMelanin.HasValue)
        {
            return 1f;
        }

        var single = Mathf.Min(fatherMelanin.Value, motherMelanin.Value);
        var single1 = Mathf.Max(fatherMelanin.Value, motherMelanin.Value);
        if (childMelanin.GetValueOrDefault() < single - 0.05f)
        {
            return 0f;
        }

        return childMelanin.GetValueOrDefault() > single1 + 0.05f ? 0f : 1f;
    }


    private static float GetParentAgeFactor(Pawn parent, Pawn child, float minAgeToHaveChildren,
        float usualAgeToHaveChildren, float maxAgeToHaveChildren)
    {
        var single = PawnRelationUtility.MaxPossibleBioAgeAt(parent.ageTracker.AgeBiologicalYearsFloat,
            parent.ageTracker.AgeChronologicalYearsFloat, child.ageTracker.AgeChronologicalYearsFloat);
        var single1 = PawnRelationUtility.MinPossibleBioAgeAt(parent.ageTracker.AgeBiologicalYearsFloat,
            child.ageTracker.AgeChronologicalYearsFloat);
        if (single <= 0f)
        {
            return 0f;
        }

        if (single1 <= single)
        {
            if (single1 <= usualAgeToHaveChildren && single >= usualAgeToHaveChildren)
            {
                return 1f;
            }

            var ageFactor = GetAgeFactor(single1, minAgeToHaveChildren, maxAgeToHaveChildren,
                usualAgeToHaveChildren);
            var ageFactor1 = GetAgeFactor(single, minAgeToHaveChildren, maxAgeToHaveChildren,
                usualAgeToHaveChildren);
            return Mathf.Max(ageFactor, ageFactor1);
        }

        if (single1 > single + 0.1f)
        {
            Log.Warning($"Min possible bio age ({single1}) is greater than max possible bio age ({single}).");
        }

        return 0f;
    }

    private static float GetAgeFactor(float ageAtBirth, float min, float max, float mid)
    {
        return GenMath.GetFactorInInterval(min, mid, max, 1.6f, ageAtBirth);
    }

    private static int NumberOfChildrenFemaleWantsEver(Pawn female)
    {
        Rand.PushState();
        Rand.Seed = female.thingIDNumber * 3;
        var num = Rand.RangeInclusive(0, 3);
        Rand.PopState();
        return num;
    }
}