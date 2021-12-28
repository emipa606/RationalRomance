using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RationalRomance_Code
{
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "BreakLoverAndFianceRelations", null)]
    public static class InteractionWorker_RomanceAttempt_BreakLoverAndFianceRelations
    {
        // CHANGE: Allowed for polyamory.
        // 1.3 change to allow multipartner precepts.
        public static bool Prefix(Pawn pawn, ref List<Pawn> oldLoversAndFiances)
        {
            oldLoversAndFiances = new List<Pawn>();
            var polyPartners = new List<(Pawn, PawnRelationDef)>();
            int num = 100;
            while ( num > 0 && !SexualityUtilities.HasFreeSpouseCapacity(pawn))
            {
                var leastLikedLover = LovePartnerRelationUtility.ExistingLeastLikedPawnWithRelation(pawn,
                    (DirectPawnRelation r) => r.def == PawnRelationDefOf.Lover && !r.otherPawn.Dead);
                if (leastLikedLover != null)
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, leastLikedLover);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, leastLikedLover);
                    oldLoversAndFiances.Add(leastLikedLover);
                    if (leastLikedLover.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) &&
                       pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous))
                    {
                        polyPartners.Add((leastLikedLover, PawnRelationDefOf.Lover));
                    }
                }
                else
                {
                    var leastLikedFiance = LovePartnerRelationUtility.ExistingLeastLikedPawnWithRelation(pawn,
                        (DirectPawnRelation r) => r.def == PawnRelationDefOf.Fiance && !r.otherPawn.Dead);
                    if (leastLikedFiance == null)
                    {
                        break;
                    }

                    if (leastLikedFiance.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) &&
                        pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous))
                    {
                        polyPartners.Add((leastLikedFiance, PawnRelationDefOf.Fiance));
                    }

                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, leastLikedFiance);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, leastLikedFiance);
                    oldLoversAndFiances.Add(leastLikedFiance);
                }
                num--;
            }
            foreach((Pawn, PawnRelationDef) p in polyPartners){
                pawn.relations.RemoveDirectRelation(PawnRelationDefOf.ExLover, p.Item1);
                pawn.relations.AddDirectRelation(p.Item2, p.Item1);
                oldLoversAndFiances.Remove(p.Item1);
            }

            return false;
        }
    }
}