using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RationalRomance_Code;

[HarmonyPatch(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover),
    nameof(ThoughtWorker_WantToSleepWithSpouseOrLover.CurrentStateInternal))]
public static class ThoughtWorker_WantToSleepWithSpouseOrLover_CurrentStateInternal
{
    // CHANGE: Allowed for polyamory.
    public static void Postfix(ref ThoughtState __result, Pawn p)
    {
        if (__result.StageIndex == ThoughtState.Inactive.StageIndex)
        {
            return;
        }

        var directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false);
        var multiplePartners =
            (from r in p.relations.PotentiallyRelatedPawns
                where LovePartnerRelationUtility.LovePartnerRelationExists(p, r)
                select r).Count() > 1;
        var partnerBedInRoom =
            (from t in p.ownership.OwnedBed.GetRoom().ContainedBeds
                where t.OwnersForReading.Contains(directPawnRelation.otherPawn)
                select t).Any();
        if (directPawnRelation != null && p.ownership.OwnedBed != null &&
            p.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) && multiplePartners && partnerBedInRoom)
        {
            __result = false;
        }
    }
}