using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RationalRomance_Code;

public class ThoughtWorker_Polyamorous : ThoughtWorker
{
    public override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!p.Spawned || !p.RaceProps.Humanlike || !p.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) ||
            !LovePartnerRelationUtility.HasAnyLovePartner(p))
        {
            return ThoughtState.Inactive;
        }

        var lovers = new List<Pawn>();
        var directRelations = p.relations.DirectRelations;
        foreach (var rel in directRelations)
        {
            if (LovePartnerRelationUtility.IsLovePartnerRelation(rel.def) && !rel.otherPawn.Dead)
            {
                lovers.Add(rel.otherPawn);
            }
        }

        if (lovers.Count == 1 && !lovers[0].story.traits.HasTrait(RRRTraitDefOf.Polyamorous))
        {
            return ThoughtState.ActiveAtStage(0);
        }

        return ThoughtState.Inactive;
    }
}