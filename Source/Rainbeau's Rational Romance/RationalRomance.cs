using HarmonyLib;
using Mlie;
using UnityEngine;
using Verse;

namespace RationalRomance_Code;

internal class RationalRomance : Mod
{
    public static Settings Settings;
    public static string CurrentVersion;

    public RationalRomance(ModContentPack content) : base(content)
    {
        new Harmony("net.rainbeau.rimworld.mod.rationalromance").PatchAll();
        Settings = GetSettings<Settings>();
        CurrentVersion =
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