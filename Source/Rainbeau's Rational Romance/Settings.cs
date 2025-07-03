using UnityEngine;
using Verse;

namespace RationalRomance_Code;

public class Settings : ModSettings
{
    public float AlienLoveChance = 33f;
    public float AsexualChance = 10f;
    public float BisexualChance = 50f;
    public float DateRate = 100f;
    public float GayChance = 20f;
    public float HookupRate = 100f;
    public float PolyChance;
    private float StraightChance = 20f;

    public void DoWindowContents(Rect canvas)
    {
        var list = new Listing_Standard
        {
            ColumnWidth = canvas.width
        };
        list.Begin(canvas);
        list.Gap(24);
        Text.Font = GameFont.Tiny;
        list.Label("RRR.Overview".Translate());
        Text.Font = GameFont.Small;
        list.Gap();
        list.Label($"{"RRR.StraightChance".Translate() + "  "}{(int)StraightChance}%");
        StraightChance = list.Slider(StraightChance, 0f, 100.99f);
        if (StraightChance > 100.99f - BisexualChance - GayChance)
        {
            StraightChance = 100.99f - BisexualChance - GayChance;
        }

        list.Gap();
        list.Label($"{"RRR.BisexualChance".Translate() + "  "}{(int)BisexualChance}%");
        BisexualChance = list.Slider(BisexualChance, 0f, 100.99f);
        if (BisexualChance > 100.99f - StraightChance - GayChance)
        {
            BisexualChance = 100.99f - StraightChance - GayChance;
        }

        list.Gap();
        list.Label($"{"RRR.GayChance".Translate() + "  "}{(int)GayChance}%");
        GayChance = list.Slider(GayChance, 0f, 100.99f);
        if (GayChance > 100.99f - StraightChance - BisexualChance)
        {
            GayChance = 100.99f - StraightChance - BisexualChance;
        }

        list.Gap();
        AsexualChance = 100 - (int)StraightChance - (int)BisexualChance - (int)GayChance;
        list.Label($"{"RRR.AsexualChance".Translate() + "  "}{AsexualChance}%");
        list.Gap(48);
        list.Label((TaggedString)$"{"RRR.PolyamoryChance".Translate() + "  "}{(int)PolyChance}%", -1f,
            "RRR.PolyamoryChanceTip".Translate());
        PolyChance = list.Slider(PolyChance, 0f, 100.99f);
        list.Gap();
        list.Label($"{"RRR.DateRate".Translate() + "  "}{(int)DateRate}%");
        DateRate = list.Slider(DateRate, 0f, 1000.99f);
        list.Gap();
        list.Label($"{"RRR.HookupRate".Translate() + "  "}{(int)HookupRate}%");
        HookupRate = list.Slider(HookupRate, 0f, 1000.99f);
        list.Gap();
        list.Label((TaggedString)$"{"RRR.AlienLoveChance".Translate() + "  "}{(int)AlienLoveChance}%", -1f,
            "RRR.AlienLoveChanceTip".Translate());
        AlienLoveChance = list.Slider(AlienLoveChance, 0f, 100.99f);
        if (RationalRomance.CurrentVersion != null)
        {
            list.Gap();
            GUI.contentColor = Color.gray;
            list.Label("RRR.CurrentModVersion".Translate(RationalRomance.CurrentVersion));
            GUI.contentColor = Color.white;
        }

        list.End();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref AsexualChance, "asexualChance", 10.0f);
        Scribe_Values.Look(ref BisexualChance, "bisexualChance", 50.0f);
        Scribe_Values.Look(ref GayChance, "gayChance", 20.0f);
        Scribe_Values.Look(ref StraightChance, "straightChance", 20.0f);
        Scribe_Values.Look(ref PolyChance, "polyChance");
        Scribe_Values.Look(ref AlienLoveChance, "alienLoveChance", 33.0f);
        Scribe_Values.Look(ref DateRate, "dateRate", 100.0f);
        Scribe_Values.Look(ref HookupRate, "hookupRate", 100.0f);
    }
}