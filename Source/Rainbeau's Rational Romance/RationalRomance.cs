using HarmonyLib;
using Mlie;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

internal class RationalRomance : Mod
{
    public static Settings Settings;
    public static string currentVersion;

    public RationalRomance(ModContentPack content) : base(content)
    {
        new Harmony("net.rainbeau.rimworld.mod.rationalromance").PatchAll();
        Settings = GetSettings<Settings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
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