using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RationalRomance_Code;

public static class SexualityUtilities
{
    public static bool IsFree(Pawn pawnToCheck)
    {
        if (PawnUtility.WillSoonHaveBasicNeed(pawnToCheck))
        {
            return false;
        }

        if (PawnUtility.EnemiesAreNearby(pawnToCheck))
        {
            return false;
        }

        if (pawnToCheck.CurJob == null)
        {
            return true;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.LayDown)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.BeatFire)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.Arrest)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.Capture)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.EscortPrisonerToBed)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.ExtinguishSelf)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.FleeAndCower)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.MarryAdjacentPawn)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.PrisonerExecution)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.ReleasePrisoner)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.Rescue)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.SocialFight)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.SpectateCeremony)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.TakeToBedToOperate)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.TakeWoundedPrisonerToBed)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.UseCommsConsole)
        {
            return false;
        }

        if (pawnToCheck.CurJob.def == JobDefOf.Vomit)
        {
            return false;
        }

        return pawnToCheck.CurJob.def != JobDefOf.Wait_Downed;
    }

    public static Pawn FindAttractivePawn(Pawn p1)
    {
        Pawn result;
        if (p1.story.traits.HasTrait(TraitDefOf.Asexual))
        {
            result = null;
        }
        else
        {
            IEnumerable<Pawn> enumerable = p1.Map.mapPawns.FreeColonistsSpawned;
            enumerable = enumerable.Except(from p in enumerable
                where (p.story.traits.HasTrait(TraitDefOf.Asexual) || !p.RaceProps.Humanlike ||
                       p.story.traits.HasTrait(TraitDefOf.Gay) && p.gender != p1.gender ||
                       p.story.traits.HasTrait(RRRTraitDefOf.Straight) && p.gender == p1.gender) &&
                      Rand.Value < 0.8
                select p);
            enumerable = from p in enumerable
                where p.Map == p1.Map && p.Faction == p1.Faction
                select p;
            if (!enumerable.Any())
            {
                result = null;
            }
            else
            {
                enumerable.TryRandomElementByWeight(
                    x => p1.relations.SecondaryRomanceChanceFactor(x) *
                         p1.relations.SecondaryRomanceChanceFactor(x), out var pawn);
                if (pawn == null)
                {
                    result = null;
                }
                else if (pawn == p1)
                {
                    result = null;
                }
                else if (LovePartnerRelationUtility.HasAnyLovePartner(pawn) && Rand.Value < 0.85f)
                {
                    result = null;
                }
                else if (LovePartnerRelationUtility.ExistingLovePartners(p1)
                         .Any(relation => relation.otherPawn == pawn))
                {
                    result = null;
                }
                else if (p1.relations.SecondaryRomanceChanceFactor(pawn) < 0.05)
                {
                    result = null;
                }
                else
                {
                    result = pawn;
                }
            }
        }

        return result;
    }

    public static Building_Bed FindHookupBed(Pawn p1, Pawn p2)
    {
        Building_Bed result;
        if (p1.ownership.OwnedBed is { SleepingSlotsCount: > 1 })
        {
            result = p1.ownership.OwnedBed;
            return result;
        }

        if (p2.ownership.OwnedBed is { SleepingSlotsCount: > 1 })
        {
            result = p2.ownership.OwnedBed;
            return result;
        }

        if (p2.ownership.OwnedBed != null)
        {
            if (p2.ownership.OwnedBed.SleepingSlotsCount <= 1)
            {
                return null;
            }

            result = p2.ownership.OwnedBed;
            return result;
        }

        foreach (var current in RestUtility.AllBedDefBestToWorst)
        {
            if (!RestUtility.CanUseBedEver(p1, current))
            {
                continue;
            }

            var building_Bed = (Building_Bed)GenClosest.ClosestThingReachable(p1.Position, p1.Map,
                ThingRequest.ForDef(current), PathEndMode.OnCell, TraverseParms.For(p1), 9999f, _ => true);
            if (building_Bed == null)
            {
                continue;
            }

            if (building_Bed.SleepingSlotsCount <= 1)
            {
                continue;
            }

            result = building_Bed;
            return result;
        }

        return null;
    }

    public static bool HasNonPolyPartner(Pawn p)
    {
        foreach (var current in p.relations.DirectRelations)
        {
            if (current.def != PawnRelationDefOf.Lover && current.def != PawnRelationDefOf.Fiance &&
                current.def != PawnRelationDefOf.Spouse)
            {
                continue;
            }

            if (current.otherPawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    public static bool IsHookupAppealing(Pawn pSubject, Pawn pObject)
    {
        bool result;
        if (PawnUtility.WillSoonHaveBasicNeed(pSubject))
        {
            result = false;
        }
        else
        {
            var num = 0f;
            num += pSubject.relations.SecondaryRomanceChanceFactor(pObject) / 1.5f;
            num *= Mathf.InverseLerp(-100f, 0f, pSubject.relations.OpinionOf(pObject));
            result = Rand.Range(0.05f, 1f) < num;
        }

        return result;
    }

    public static bool WillPawnTryHookup(Pawn p1)
    {
        bool result;
        if (p1.story.traits.HasTrait(TraitDefOf.Asexual))
        {
            result = false;
        }
        else
        {
            var pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(p1, false);
            if (pawn != null)
            {
                float num = p1.relations.OpinionOf(pawn);
                float num2;
                if (p1.story.traits.HasTrait(RRRTraitDefOf.Philanderer))
                {
                    num2 = p1.Map == pawn.Map
                        ? Mathf.InverseLerp(70f, 15f, num)
                        : Mathf.InverseLerp(100f, 50f, num);
                }
                else
                {
                    num2 = Mathf.InverseLerp(30f, -80f, num);
                }

                if (p1.story.traits.HasTrait(RRRTraitDefOf.Faithful))
                {
                    num2 = 0f;
                }

                num2 /= 2f;
                result = Rand.Range(0f, 1f) < num2;
            }
            else
            {
                result = true;
            }
        }

        return result;
    }

    public static string GetAdultCulturalAdjective(Pawn p)
    {
        if (p?.story?.Adulthood == null || p.story.Childhood == null)
        {
            return "Colonial";
        }

        if (p.story.Adulthood.spawnCategories?.Contains("Tribal") == true ||
            p.story.Adulthood.description.ToLower().Contains("tribe"))
        {
            return "Tribal";
        }

        if (p.story.Adulthood.title.ToLower().Contains("medieval") ||
            p.story.Adulthood.description.ToLower().Contains("medieval") ||
            p.story.Adulthood.description.ToLower().Contains("village"))
        {
            return "Medieval";
        }

        if ((p.story.Adulthood.title.ToLower().Contains("glitterworld") ||
             p.story.Adulthood.description.ToLower().Contains("glitterworld")) &&
            p.story.Adulthood.title != "adventurer")
        {
            return "Glitterworld";
        }

        if (p.story.Adulthood.title.Contains("urbworld") || p.story.Adulthood.title.Contains("vatgrown") ||
            p.story.Adulthood.description.ToLower().Contains("urbworld"))
        {
            return "Urbworld";
        }

        if (p.story.Adulthood.title.Contains("midworld") ||
            p.story.Adulthood.description.ToLower().Contains("midworld"))
        {
            return "Midworld";
        }

        if (p.story.Adulthood.title.Contains("imperial") ||
            p.story.Adulthood.description.ToLower().Contains("imperial"))
        {
            return "Imperial";
        }

        return "Colonial";
    }

    public static bool IsPsychicLoveActive(Pawn initiator, Pawn recipient)
    {
        var psylove = (HediffWithTarget)initiator.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicLove);
        return psylove != null && psylove.target == recipient;
    }

    public static bool HasFreeLoverCapacity(Pawn pawn)
    {
        return IdeoUtility.DoerWillingToDo(pawn.GetHistoryEventForLoveRelationCountPlusOne(), pawn);
    }

    public static bool HasFreeSpouseFianceCapacity(Pawn pawn)
    {
        return IdeoUtility.DoerWillingToDo(pawn.GetHistoryEventForSpouseAndFianceCountPlusOne(), pawn);
    }
}