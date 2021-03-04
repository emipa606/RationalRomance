using HarmonyLib;
using UnityEngine;
using Verse;

namespace RationalRomance_Code
{
    internal class RationalRomance : Mod
    {
        public static Settings Settings;

        public RationalRomance(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("net.rainbeau.rimworld.mod.rationalromance");
            harmony.PatchAll();
            Settings = GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "RRR.RationalRomance".Translate();
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }
    }
}