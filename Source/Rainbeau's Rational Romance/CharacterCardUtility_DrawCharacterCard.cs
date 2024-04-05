using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard", typeof(Rect), typeof(Pawn), typeof(Action),
    typeof(Rect), typeof(bool))]
internal static class CharacterCardUtility_DrawCharacterCard
{
    // CHANGE: Allowed more traits to be displayed, if "More Trait Slots" isn't already doing so, anyway.
    [HarmonyPriority(Priority.VeryHigh)]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var l = new List<CodeInstruction>(instructions);
        if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("More Trait Slots")))
        {
            return l;
        }

        for (var i = 0; i < l.Count; ++i)
        {
            if (l[i].opcode != OpCodes.Ldstr || !l[i].operand.Equals("Traits"))
            {
                continue;
            }

            for (var j = i; j >= i - 20; --j)
            {
                if (l[j].opcode != OpCodes.Ldc_R4)
                {
                    continue;
                }

                if (!float.TryParse(l[j].operand.ToString(), out var temp))
                {
                    continue;
                }

                if (temp != 100f)
                {
                    continue;
                }

                l[j].operand = 80f;
                break;
            }

            for (var j = i; i >= i - 20; --j)
            {
                if (l[j].opcode != OpCodes.Ldc_I4_2)
                {
                    continue;
                }

                l[j].opcode = OpCodes.Ldc_I4_1;
                break;
            }

            bool first0 = false,
                first30 = false,
                first24 = false;
            for (; i < l.Count; ++i)
            {
                if (l[i].opcode == OpCodes.Ldc_R4 && l[i].operand != null)
                {
                    if (!float.TryParse(l[i].operand.ToString(), out var f))
                    {
                        continue;
                    }

                    if (!first30 && f == 30f)
                    {
                        l[i].operand = 24f;
                    }
                    else if (!first24 && f == 24f)
                    {
                        first24 = true;
                        l[i].operand = 16f;
                        break;
                    }
                }
                else if (!first0 && l[i].opcode == OpCodes.Ldc_I4_1)
                {
                    first0 = true;
                    l[i].opcode = OpCodes.Ldc_I4_0;
                }
            }

            i = int.MaxValue - 1;
        }

        return l;
    }
}