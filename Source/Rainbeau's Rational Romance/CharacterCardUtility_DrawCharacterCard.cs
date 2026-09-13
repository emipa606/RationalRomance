using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(CharacterCardUtility), nameof(CharacterCardUtility.DrawCharacterCard), typeof(Rect), typeof(Pawn),
    typeof(Action),
    typeof(Rect), typeof(bool))]
internal static class CharacterCardUtility_DrawCharacterCard
{
    private const int SearchWindowSize = 20;
    private const float FloatComparisonEpsilon = 0.001f;

    // CHANGE: Allowed more traits to be displayed, if "More Trait Slots" isn't already doing so, anyway.
    [HarmonyPriority(Priority.VeryHigh)]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("More Trait Slots")))
        {
            return instructionsList;
        }

        var traitsLabelIndex = instructionsList.FindIndex(instruction =>
            instruction.opcode == OpCodes.Ldstr &&
            instruction.operand is "Traits");

        if (traitsLabelIndex < 0)
        {
            return instructionsList;
        }

        AdjustTraitsHeaderWidth(instructionsList, traitsLabelIndex);
        AdjustTraitsVisibleRows(instructionsList, traitsLabelIndex);
        AdjustTraitsSectionLayout(instructionsList, traitsLabelIndex);

        return instructionsList;
    }

    private static void AdjustTraitsHeaderWidth(List<CodeInstruction> instructions, int traitsLabelIndex)
    {
        for (var i = traitsLabelIndex; i >= 0 && i >= traitsLabelIndex - SearchWindowSize; i--)
        {
            if (instructions[i].opcode != OpCodes.Ldc_R4 || !TryGetFloatOperand(instructions[i], out var value))
            {
                continue;
            }

            if (!ApproximatelyEquals(value, 100f))
            {
                continue;
            }

            instructions[i].operand = 80f;
            return;
        }
    }

    private static void AdjustTraitsVisibleRows(List<CodeInstruction> instructions, int traitsLabelIndex)
    {
        for (var i = traitsLabelIndex; i >= 0 && i >= traitsLabelIndex - SearchWindowSize; i--)
        {
            if (instructions[i].opcode != OpCodes.Ldc_I4_2)
            {
                continue;
            }

            instructions[i].opcode = OpCodes.Ldc_I4_1;
            return;
        }
    }

    private static void AdjustTraitsSectionLayout(List<CodeInstruction> instructions, int traitsLabelIndex)
    {
        var replacedFirstOneConstant = false;
        var replacedThirtyConstant = false;

        for (var i = traitsLabelIndex; i < instructions.Count; i++)
        {
            if (!replacedFirstOneConstant && instructions[i].opcode == OpCodes.Ldc_I4_1)
            {
                instructions[i].opcode = OpCodes.Ldc_I4_0;
                replacedFirstOneConstant = true;
                continue;
            }

            if (instructions[i].opcode != OpCodes.Ldc_R4 || !TryGetFloatOperand(instructions[i], out var value))
            {
                continue;
            }

            if (!replacedThirtyConstant && ApproximatelyEquals(value, 30f))
            {
                instructions[i].operand = 24f;
                replacedThirtyConstant = true;
                continue;
            }

            if (!ApproximatelyEquals(value, 24f))
            {
                continue;
            }

            instructions[i].operand = 16f;
            return;
        }
    }

    private static bool TryGetFloatOperand(CodeInstruction instruction, out float value)
    {
        switch (instruction.operand)
        {
            case float floatValue:
                value = floatValue;
                return true;
            case double doubleValue:
                value = (float)doubleValue;
                return true;
            default:
                value = 0f;
                return false;
        }
    }

    private static bool ApproximatelyEquals(float left, float right)
    {
        return Math.Abs(left - right) < FloatComparisonEpsilon;
    }
}