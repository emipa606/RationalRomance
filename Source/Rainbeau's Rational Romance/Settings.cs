using UnityEngine;
using Verse;

namespace RationalRomance_Code;

public class Settings : ModSettings
{
    public float alienLoveChance = 33f;
    public float asexualChance = 10f;
    public float bisexualChance = 50f;
    public float dateRate = 100f;
    public float gayChance = 20f;
    public float hookupRate = 100f;
    public float polyChance;
    public float straightChance = 20f;

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
        list.Label("RRR.StraightChance".Translate() + "  " + (int)straightChance + "%");
        straightChance = list.Slider(straightChance, 0f, 100.99f);
        if (straightChance > 100.99f - bisexualChance - gayChance)
        {
            straightChance = 100.99f - bisexualChance - gayChance;
        }

        list.Gap();
        list.Label("RRR.BisexualChance".Translate() + "  " + (int)bisexualChance + "%");
        bisexualChance = list.Slider(bisexualChance, 0f, 100.99f);
        if (bisexualChance > 100.99f - straightChance - gayChance)
        {
            bisexualChance = 100.99f - straightChance - gayChance;
        }

        list.Gap();
        list.Label("RRR.GayChance".Translate() + "  " + (int)gayChance + "%");
        gayChance = list.Slider(gayChance, 0f, 100.99f);
        if (gayChance > 100.99f - straightChance - bisexualChance)
        {
            gayChance = 100.99f - straightChance - bisexualChance;
        }

        list.Gap();
        asexualChance = 100 - (int)straightChance - (int)bisexualChance - (int)gayChance;
        list.Label("RRR.AsexualChance".Translate() + "  " + asexualChance + "%");
        list.Gap(48);
        list.Label("RRR.PolyamoryChance".Translate() + "  " + (int)polyChance + "%", -1f,
            "RRR.PolyamoryChanceTip".Translate());
        polyChance = list.Slider(polyChance, 0f, 100.99f);
        list.Gap();
        list.Label("RRR.DateRate".Translate() + "  " + (int)dateRate + "%");
        dateRate = list.Slider(dateRate, 0f, 1000.99f);
        list.Gap();
        list.Label("RRR.HookupRate".Translate() + "  " + (int)hookupRate + "%");
        hookupRate = list.Slider(hookupRate, 0f, 1000.99f);
        list.Gap();
        list.Label("RRR.AlienLoveChance".Translate() + "  " + (int)alienLoveChance + "%", -1f,
            "RRR.AlienLoveChanceTip".Translate());
        alienLoveChance = list.Slider(alienLoveChance, 0f, 100.99f);
        list.End();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref asexualChance, "asexualChance", 10.0f);
        Scribe_Values.Look(ref bisexualChance, "bisexualChance", 50.0f);
        Scribe_Values.Look(ref gayChance, "gayChance", 20.0f);
        Scribe_Values.Look(ref straightChance, "straightChance", 20.0f);
        Scribe_Values.Look(ref polyChance, "polyChance");
        Scribe_Values.Look(ref alienLoveChance, "alienLoveChance", 33.0f);
        Scribe_Values.Look(ref dateRate, "dateRate", 100.0f);
        Scribe_Values.Look(ref hookupRate, "hookupRate", 100.0f);
    }
}