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
        public static bool Prefix(Pawn pawn, ref List<Pawn> oldLoversAndFiances)
        {
            oldLoversAndFiances = new List<Pawn>();
            while (true)
            {
                var firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
                if (firstDirectRelationPawn != null &&
                    (!firstDirectRelationPawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) ||
                     !pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous)))
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, firstDirectRelationPawn);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn);
                    oldLoversAndFiances.Add(firstDirectRelationPawn);
                }
                else
                {
                    var firstDirectRelationPawn1 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
                    if (firstDirectRelationPawn1 == null)
                    {
                        break;
                    }

                    if (firstDirectRelationPawn1.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) &&
                        pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous))
                    {
                        continue;
                    }

                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, firstDirectRelationPawn1);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn1);
                    oldLoversAndFiances.Add(firstDirectRelationPawn1);
                }
            }

            return false;
        }
    }
}