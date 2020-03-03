using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI;
	
namespace RationalRomance_Code {

	public class Controller : Mod {
		public static Settings Settings;
		public override string SettingsCategory() { return "RRR.RationalRomance".Translate(); }
		public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
		public Controller(ModContentPack content) : base(content) {
			var harmony = new Harmony("net.rainbeau.rimworld.mod.rationalromance");
			harmony.PatchAll( Assembly.GetExecutingAssembly() );
			Settings = GetSettings<Settings>();
		}
	}

	public class Settings : ModSettings {
		public float asexualChance = 10f;
		public float bisexualChance = 50f;
		public float gayChance = 20f;
		public float straightChance = 20f;
		public float polyChance = 0f;
		public float alienLoveChance = 33f;
		public float dateRate = 100f;
		public float hookupRate = 100f;
		public void DoWindowContents(Rect canvas) {
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = canvas.width;
			list.Begin(canvas);
			list.Gap(24);
			Text.Font = GameFont.Tiny;
			list.Label("RRR.Overview".Translate());
			Text.Font = GameFont.Small;
			list.Gap();
			list.Label("RRR.StraightChance".Translate()+"  "+(int)straightChance+"%");
			straightChance = list.Slider(straightChance, 0f, 100.99f);
			if (straightChance > 100.99f-bisexualChance-gayChance) {
				straightChance = 100.99f-bisexualChance-gayChance;
			}
			list.Gap();
			list.Label("RRR.BisexualChance".Translate()+"  "+(int)bisexualChance+"%");
			bisexualChance = list.Slider(bisexualChance, 0f, 100.99f);
			if (bisexualChance > 100.99f-straightChance-gayChance) {
				bisexualChance = 100.99f-straightChance-gayChance;
			}
			list.Gap();
			list.Label("RRR.GayChance".Translate()+"  "+(int)gayChance+"%");
			gayChance = list.Slider(gayChance, 0f, 100.99f);
			if (gayChance > 100.99f-straightChance-bisexualChance) {
				gayChance = 100.99f-straightChance-bisexualChance;
			}
			list.Gap();
			asexualChance = 100-(int)straightChance-(int)bisexualChance-(int)gayChance;
			list.Label("RRR.AsexualChance".Translate()+"  "+asexualChance+"%");
			list.Gap(48);
			list.Label("RRR.PolyamoryChance".Translate()+"  "+(int)polyChance+"%", -1f, "RRR.PolyamoryChanceTip".Translate());
			polyChance = list.Slider(polyChance, 0f, 100.99f);
			list.Gap();
			list.Label("RRR.DateRate".Translate()+"  "+(int)dateRate+"%");
			dateRate = list.Slider(dateRate, 0f, 200.99f);
			list.Gap();
			list.Label("RRR.HookupRate".Translate()+"  "+(int)hookupRate+"%");
			hookupRate = list.Slider(hookupRate, 0f, 200.99f);
			list.Gap();
			list.Label("RRR.AlienLoveChance".Translate()+"  "+(int)alienLoveChance+"%", -1f, "RRR.AlienLoveChanceTip".Translate());
			alienLoveChance = list.Slider(alienLoveChance, 0f, 100.99f);
			list.End();
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref asexualChance, "asexualChance", 10.0f);
			Scribe_Values.Look(ref bisexualChance, "bisexualChance", 50.0f);
			Scribe_Values.Look(ref gayChance, "gayChance", 20.0f);
			Scribe_Values.Look(ref straightChance, "straightChance", 20.0f);
			Scribe_Values.Look(ref polyChance, "polyChance", 0.0f);
			Scribe_Values.Look(ref alienLoveChance, "alienLoveChance", 33.0f);
			Scribe_Values.Look(ref dateRate, "dateRate", 100.0f);
			Scribe_Values.Look(ref hookupRate, "hookupRate", 100.0f);
		}
	}

	[DefOf]
	public static class RRRJobDefOf {
		public static JobDef DoLovinCasual;
		public static JobDef JobDateFollow;
		public static JobDef JobDateLead;
		public static JobDef LeadHookup;
		public static JobDef ProposeDate;
	}

	[DefOf]
	public static class RRRMiscDefOf {
		public static InteractionDef TriedHookupWith;
		public static JoyKindDef Lewd;
		public static JoyKindDef Social;
		public static RulePackDef HookupSucceeded;
		public static RulePackDef HookupFailed;
	}

	[DefOf]
	public static class RRRThoughtDefOf {
		public static ThoughtDef FailedHookupAttemptOnMe;
		public static ThoughtDef FeelingHarassed;
		public static ThoughtDef RebuffedMyHookupAttempt;
	}
	
	[DefOf]
	public static class RRRTraitDefOf {
		public static TraitDef Asexual;
		public static TraitDef Bisexual;
		public static TraitDef Faithful;
		public static TraitDef Philanderer;
		public static TraitDef Polyamorous;
		public static TraitDef Straight;
	}

	public static class ExtraTraits {
		public static void AssignOrientation(Pawn pawn) {
			float orientation = Rand.Value;
			if (pawn.gender == Gender.None) { return; }
			if (orientation < (Controller.Settings.asexualChance / 100)) {
				if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn)) {
					pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Bisexual, 0, false));
				}
				else if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)) {
					pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Bisexual, 0, false));
				}
				else if (pawn.story.traits.HasTrait(RRRTraitDefOf.Philanderer)) {
					pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Bisexual, 0, false));
				}
				else {
					pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Asexual, 0, false));
				}
			}
			else if (orientation < ((Controller.Settings.asexualChance + Controller.Settings.bisexualChance) / 100)) {
				pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Bisexual, 0, false));
			}
			else if (orientation < ((Controller.Settings.asexualChance + Controller.Settings.bisexualChance + Controller.Settings.gayChance) / 100)) {
				if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn)) {
					pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Bisexual, 0, false));
				}
				else {
					pawn.story.traits.GainTrait(new Trait(TraitDefOf.Gay, 0, false));
				}
			}
			else {
				if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)) {
					pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Bisexual, 0, false));
				}
				else {
					pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Straight, 0, false));
				}
			}
			if (!pawn.story.traits.HasTrait(RRRTraitDefOf.Asexual) && !pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous)) {
				if (Rand.Value < (Controller.Settings.polyChance / 100)) {
					pawn.story.traits.GainTrait(new Trait(RRRTraitDefOf.Polyamorous, 0, false));
				}
			}
			return;
		}
	}

//
// HARMONY PATCHES
//

	[HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard", new Type[] { typeof(Rect), typeof(Pawn), typeof(Action), typeof(Rect) })]
	static class CharacterCardUtility_DrawCharacterCard {
		// CHANGE: Allowed more traits to be displayed, if "More Trait Slots" isn't already doing so, anyway.
		[HarmonyPriority(Priority.VeryHigh)]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			MethodInfo SetTextSize = AccessTools.Method(typeof(CharacterCardUtility_DrawCharacterCard), "SetTextSize");
			List<CodeInstruction> l = new List<CodeInstruction>(instructions);
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("More Trait Slots"))) {
				return l;
			}
			for (int i = 0; i < l.Count; ++i) {
				if (l[i].opcode == OpCodes.Ldstr && l[i].operand.Equals("Traits")) {
					for (int j = i; j >= i - 20; --j) {
						if (l[j].opcode == OpCodes.Ldc_R4) {
							float temp;
							if (float.TryParse(l[j].operand.ToString(), out temp)) {
								if (temp == 100f) {
									l[j].operand = 80f;
									break;
								}
							}
						}
					}
					for (int j = i; i >= i - 20; --j) {
						if (l[j].opcode == OpCodes.Ldc_I4_2) {
							l[j].opcode = OpCodes.Ldc_I4_1;
							break;
						}
					}
					bool first0 = false,
					first30 = false,
					first24 = false;
					for (; i < l.Count; ++i) {
						if (l[i].opcode == OpCodes.Ldc_R4 && l[i].operand != null) {
							float f;
							if (float.TryParse(l[i].operand.ToString(), out f)) {
								if (!first30 && f == 30f) {
									l[i].operand = 24f;
								}
								else if (!first24 && f == 24f) {
									first24 = true;
									l[i].operand = 16f;
									break;
								}
							}
						}
						else if (!first0 && l[i].opcode == OpCodes.Ldc_I4_1) {
							first0 = true;
							l[i].opcode = OpCodes.Ldc_I4_0;
						}
					}
					i = int.MaxValue - 1;
				}
			}
			return l;
		}
	}

	[HarmonyPatch(typeof(ChildRelationUtility), "ChanceOfBecomingChildOf", null)]
	public static class ChildRelationUtility_ChanceOfBecomingChildOf {
		// CHANGE: Removed bias against gays being assigned as parents.
		public static bool Prefix(Pawn child, Pawn father, Pawn mother, PawnGenerationRequest? childGenerationRequest, PawnGenerationRequest? fatherGenerationRequest, PawnGenerationRequest? motherGenerationRequest, ref float __result) {
			if (father != null && father.gender != Gender.Male) {
				Log.Warning(string.Concat("Tried to calculate chance for father with gender \"", father.gender, "\"."), false);
				__result = 0f;
				return false;
			}
			if (mother != null && mother.gender != Gender.Female) {
				Log.Warning(string.Concat("Tried to calculate chance for mother with gender \"", mother.gender, "\"."), false);
				__result = 0f;
				return false;
			}
			if (father != null && child.GetFather() != null && child.GetFather() != father) {
				__result = 0f;
				return false;
			}
			if (mother != null && child.GetMother() != null && child.GetMother() != mother) {
				__result = 0f;
				return false;
			}
			if (mother != null && father != null && !LovePartnerRelationUtility.LovePartnerRelationExists(mother, father) && !LovePartnerRelationUtility.ExLovePartnerRelationExists(mother, father)) {
				__result = 0f;
				return false;
			}
			float? melanin = GetMelanin(child, childGenerationRequest);
			float? nullable = GetMelanin(father, fatherGenerationRequest);
			float? melanin1 = GetMelanin(mother, motherGenerationRequest);
			bool flag = (father == null ? false : child.GetFather() != father);
			float skinColorFactor = GetSkinColorFactor(melanin, nullable, melanin1, flag, (mother == null ? false : child.GetMother() != mother));
			if (skinColorFactor <= 0f) {
				__result = 0f;
				return false;
			}
			float parentAgeFactor = 1f;
			float single = 1f;
			float childrenCount = 1f;
			float single1 = 1f;
			if (father != null && child.GetFather() == null) {
				parentAgeFactor = GetParentAgeFactor(father, child, 14f, 30f, 50f);
				if (parentAgeFactor == 0f) {
					__result = 0f;
					return false;
				}
			}
			if (mother != null && child.GetMother() == null) {
				single = GetParentAgeFactor(mother, child, 16f, 27f, 45f);
				if (single == 0f) {
					__result = 0f;
					return false;
				}
				int num = NumberOfChildrenFemaleWantsEver(mother);
				if (mother.relations.ChildrenCount >= num) {
					__result = 0f;
					return false;
				}
				childrenCount = 1f - (float)mother.relations.ChildrenCount / (float)num;
			}
			float single2 = 1f;
			if (mother != null) {
				Pawn firstDirectRelationPawn = mother.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
				if (firstDirectRelationPawn != null && firstDirectRelationPawn != father) {
					single2 *= 0.15f;
				}
			}
			if (father != null) {
				Pawn pawn = father.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
				if (pawn != null && pawn != mother) {
					single2 *= 0.15f;
				}
			}
			__result = skinColorFactor * parentAgeFactor * single * childrenCount * single2 * single1;
			return false;
		}
		private static float? GetMelanin(Pawn pawn, PawnGenerationRequest? request) {
			if (request.HasValue) {
				return request.Value.FixedMelanin;
			}
			if (pawn == null) {
				return null;
			}
			return new float?(pawn.story.melanin);
		}
		private static float GetSkinColorFactor(float? childMelanin, float? fatherMelanin, float? motherMelanin, bool fatherIsNew, bool motherIsNew) {
			if (childMelanin.HasValue && fatherMelanin.HasValue && motherMelanin.HasValue) {
				float single = Mathf.Min(fatherMelanin.Value, motherMelanin.Value);
				float single1 = Mathf.Max(fatherMelanin.Value, motherMelanin.Value);
				if ((!childMelanin.HasValue ? false : childMelanin.GetValueOrDefault() < single - 0.05f)) {
					return 0f;
				}
				if ((!childMelanin.HasValue ? false : childMelanin.GetValueOrDefault() > single1 + 0.05f)) {
					return 0f;
				}
			}
			float newParentSkinColorFactor = 1f;
			if (fatherIsNew) {
				newParentSkinColorFactor *= GetNewParentSkinColorFactor(fatherMelanin, motherMelanin, childMelanin);
			}
			if (motherIsNew) {
				newParentSkinColorFactor *= GetNewParentSkinColorFactor(motherMelanin, fatherMelanin, childMelanin);
			}
			return newParentSkinColorFactor;
		}
		private static float GetNewParentSkinColorFactor(float? newParentMelanin, float? otherParentMelanin, float? childMelanin) {
			if (!newParentMelanin.HasValue) {
				if (!otherParentMelanin.HasValue) {
					if (!childMelanin.HasValue) {
						return 1f;
					}
					return PawnSkinColors.GetMelaninCommonalityFactor(childMelanin.Value);
				}
				if (!childMelanin.HasValue) {
					return PawnSkinColors.GetMelaninCommonalityFactor(otherParentMelanin.Value);
				}
				float reflectedSkin = ChildRelationUtility.GetReflectedSkin(otherParentMelanin.Value, childMelanin.Value);
				return PawnSkinColors.GetMelaninCommonalityFactor(reflectedSkin);
			}
			if (!otherParentMelanin.HasValue) {
				if (!childMelanin.HasValue) {
					return PawnSkinColors.GetMelaninCommonalityFactor(newParentMelanin.Value);
				}
				return ChildRelationUtility.GetMelaninSimilarityFactor(newParentMelanin.Value, childMelanin.Value);
			}
			if (childMelanin.HasValue) {
				float single = ChildRelationUtility.GetReflectedSkin(otherParentMelanin.Value, childMelanin.Value);
				return ChildRelationUtility.GetMelaninSimilarityFactor(newParentMelanin.Value, single);
			}
			float value = (newParentMelanin.Value + otherParentMelanin.Value) / 2f;
			return PawnSkinColors.GetMelaninCommonalityFactor(value);
		}
		private static float GetParentAgeFactor(Pawn parent, Pawn child, float minAgeToHaveChildren, float usualAgeToHaveChildren, float maxAgeToHaveChildren) {
			float single = PawnRelationUtility.MaxPossibleBioAgeAt(parent.ageTracker.AgeBiologicalYearsFloat, parent.ageTracker.AgeChronologicalYearsFloat, child.ageTracker.AgeChronologicalYearsFloat);
			float single1 = PawnRelationUtility.MinPossibleBioAgeAt(parent.ageTracker.AgeBiologicalYearsFloat, child.ageTracker.AgeChronologicalYearsFloat);
			if (single <= 0f) {
				return 0f;
			}
			if (single1 <= single) {
				if (single1 <= usualAgeToHaveChildren && single >= usualAgeToHaveChildren) {
					return 1f;
				}
				float ageFactor = GetAgeFactor(single1, minAgeToHaveChildren, maxAgeToHaveChildren, usualAgeToHaveChildren);
				float ageFactor1 = GetAgeFactor(single, minAgeToHaveChildren, maxAgeToHaveChildren, usualAgeToHaveChildren);
				return Mathf.Max(ageFactor, ageFactor1);
			}
			if (single1 > single + 0.1f) {
				Log.Warning(string.Concat(new object[] { "Min possible bio age (", single1, ") is greater than max possible bio age (", single, ")." }), false);
			}
			return 0f;
		}
		private static float GetAgeFactor(float ageAtBirth, float min, float max, float mid) {
			return GenMath.GetFactorInInterval(min, mid, max, 1.6f, ageAtBirth);
		}
		private static int NumberOfChildrenFemaleWantsEver(Pawn female) {
			Rand.PushState();
			Rand.Seed = female.thingIDNumber * 3;
			int num = Rand.RangeInclusive(0, 3);
			Rand.PopState();
			return num;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_Breakup), "RandomSelectionWeight", null)]
	public static class InteractionWorker_Breakup_RandomSelectionWeight {
		// CHANGE: Pawns are more likely to break up if currently with non-ideal partner.
		public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result) {
			if (!initiator.story.traits.HasTrait(RRRTraitDefOf.Asexual) && !initiator.story.traits.HasTrait(RRRTraitDefOf.Bisexual) && !initiator.story.traits.HasTrait(TraitDefOf.Gay) && !initiator.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
				ExtraTraits.AssignOrientation(initiator);
			}
			if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient)) {
				__result = 0f;
				return false;
			}
			float single = Mathf.InverseLerp(100f, -100f, (float)initiator.relations.OpinionOf(recipient));
			float single1 = 1f;
			if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient)) {
				single1 = 0.4f;
			}
			__result = 0.02f * single * single1;
			if ((initiator.gender == recipient.gender) && (initiator.story.traits.HasTrait(RRRTraitDefOf.Straight))) {
				__result *= 2f;
			}
			if ((initiator.gender != recipient.gender) && (initiator.story.traits.HasTrait(TraitDefOf.Gay))) {
				__result *= 2f;
			}
			if (initiator.story.traits.HasTrait(RRRTraitDefOf.Asexual)) {
				__result *= 2f;
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), "AcceptanceChance", null)]
	public static class InteractionWorker_MarriageProposal_AcceptanceChance {
		// CHANGE: Pawns will always reject marriage proposals if proposer is of non-ideal gender.
		public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result) {
			if (!recipient.story.traits.HasTrait(RRRTraitDefOf.Asexual) && !recipient.story.traits.HasTrait(RRRTraitDefOf.Bisexual) && !recipient.story.traits.HasTrait(TraitDefOf.Gay) && !recipient.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
				ExtraTraits.AssignOrientation(recipient);
			}
			if ((initiator.gender == recipient.gender) && (recipient.story.traits.HasTrait(RRRTraitDefOf.Straight))) {
				__result = 0f;
				return false;
			}
			if ((initiator.gender != recipient.gender) && (recipient.story.traits.HasTrait(TraitDefOf.Gay))) {
				__result = 0f;
				return false;
			}
			if (recipient.story.traits.HasTrait(RRRTraitDefOf.Asexual)) {
				__result = 0f;
				return false;
			}
			float single = 0.9f;
			single *= Mathf.Clamp01(GenMath.LerpDouble(-20f, 60f, 0f, 1f, (float)recipient.relations.OpinionOf(initiator)));
			__result = Mathf.Clamp01(single);
			return false;
		}
	}
	
	[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), "RandomSelectionWeight", null)]
	public static class InteractionWorker_MarriageProposal_RandomSelectionWeight {
		// CHANGE: Female pawns are now just as likely to propose as male pawns, with cultural variations.
		// CHANGE: Marriage won't be proposed to someone of non-ideal gender.
		public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result) {
			if (!initiator.story.traits.HasTrait(RRRTraitDefOf.Asexual) && !initiator.story.traits.HasTrait(RRRTraitDefOf.Bisexual) && !initiator.story.traits.HasTrait(TraitDefOf.Gay) && !initiator.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
				ExtraTraits.AssignOrientation(initiator);
			}
			DirectPawnRelation directRelation = initiator.relations.GetDirectRelation(PawnRelationDefOf.Lover, recipient);
			if (directRelation == null) {
				__result = 0f;
				return false;
			}
			Pawn spouse = recipient.GetSpouse();
			Pawn pawn = initiator.GetSpouse();
			if (spouse != null && !spouse.Dead || pawn != null && !pawn.Dead) {
				__result = 0f;
				return false;
			}
			if ((initiator.gender == recipient.gender) && (initiator.story.traits.HasTrait(RRRTraitDefOf.Straight))) {
				__result = 0f;
				return false;
			}
			if ((initiator.gender != recipient.gender) && (initiator.story.traits.HasTrait(TraitDefOf.Gay))) {
				__result = 0f;
				return false;
			}
			if (initiator.story.traits.HasTrait(RRRTraitDefOf.Asexual)) {
				__result = 0f;
				return false;
			}
			float genderAggressiveness;
			string backgroundCulture = SexualityUtilities.GetAdultCulturalAdjective(initiator);
			if (backgroundCulture == "Urbworld") {
				genderAggressiveness = (initiator.gender != Gender.Male ? 0.75f : 1f);
			}
			else if (backgroundCulture == "Imperial") {
				genderAggressiveness = (initiator.gender != Gender.Female ? 0.75f : 1f);
			}
			else if (backgroundCulture == "Tribal") {
				genderAggressiveness = (initiator.gender != Gender.Female ? 0.2f : 1f);
			}
			else if (backgroundCulture == "Medieval") {
				genderAggressiveness = (initiator.gender != Gender.Male ? 0.2f : 1f);
			}
			else {
				genderAggressiveness = 1f;
			}
			float single = 0.4f;
			int ticksGame = Find.TickManager.TicksGame;
			float single1 = (float)(ticksGame - directRelation.startTicks) / 60000f;
			single *= Mathf.InverseLerp(0f, 60f, single1);
			single *= Mathf.InverseLerp(0f, 60f, (float)initiator.relations.OpinionOf(recipient));
			if (recipient.relations.OpinionOf(initiator) < 0) {
				single *= 0.3f;
			}
			__result = single * genderAggressiveness;
			return false;
		}
	}
	
	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "BreakLoverAndFianceRelations", null)]
	public static class InteractionWorker_RomanceAttempt_BreakLoverAndFianceRelations {
		// CHANGE: Allowed for polyamory.
		public static bool Prefix(Pawn pawn, ref List<Pawn> oldLoversAndFiances) {
			oldLoversAndFiances = new List<Pawn>();
			while (true) {
				Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
				if (firstDirectRelationPawn != null && (!firstDirectRelationPawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) || !pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous))) {
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, firstDirectRelationPawn);
					pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn);
					oldLoversAndFiances.Add(firstDirectRelationPawn);
				}
				else {
					Pawn firstDirectRelationPawn1 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
					if (firstDirectRelationPawn1 == null) {
						break;
					}
					else if (!firstDirectRelationPawn1.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) || !pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous)) {
						pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, firstDirectRelationPawn1);
						pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn1);
						oldLoversAndFiances.Add(firstDirectRelationPawn1);
					}
				}
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight", null)]
	public static class InteractionWorker_RomanceAttempt_RandomSelectionWeight {
		// CHANGE: Updated with new orientation options and traits.
		// CHANGE: Women are no less likely than men to initiate romance, when from colonist or glitterworld cultures.
		// CHANGE: Women are more forward when from tribal or imperial; men when from medieval or urbworld cultures. 
		// CHANGE: Pawn in mental break or who is already lover of initiator can't be targeted.
		// CHANGE: Pawn can't perform romance attempt if recently rebuffed.
		// CHANGE: Pawn can't target others or be targeted if current lover is good enough.
		// CHANGE: Allowed for polyamory.
		public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result) {
			if (!initiator.story.traits.HasTrait(RRRTraitDefOf.Asexual) && !initiator.story.traits.HasTrait(RRRTraitDefOf.Bisexual) && !initiator.story.traits.HasTrait(TraitDefOf.Gay) && !initiator.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
				ExtraTraits.AssignOrientation(initiator);
			}
			if (!recipient.story.traits.HasTrait(RRRTraitDefOf.Asexual) && !recipient.story.traits.HasTrait(RRRTraitDefOf.Bisexual) && !recipient.story.traits.HasTrait(TraitDefOf.Gay) && !recipient.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
				ExtraTraits.AssignOrientation(recipient);
			}
			if (recipient.InMentalState || LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient)) {
				__result = 0f;
				return false;
			}
			if (initiator.needs.mood.thoughts.memories.NumMemoriesOfDef(ThoughtDefOf.RebuffedMyRomanceAttempt) > 0) {
				__result = 0f;
				return false;
			}
			Pawn initiator_partner = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, false);
			if (initiator_partner != null && initiator.relations.OpinionOf(initiator_partner) >= 33) {
				if (!initiator.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) && !initiator.story.traits.HasTrait(RRRTraitDefOf.Philanderer)) {
					__result = 0f;
					return false;
				}
			}
			Pawn recipient_partner = LovePartnerRelationUtility.ExistingMostLikedLovePartner(recipient, false);
			if (recipient_partner != null && recipient.relations.OpinionOf(recipient_partner) >= 33) {
				if (!recipient.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) && !recipient.story.traits.HasTrait(RRRTraitDefOf.Philanderer)) {
					__result = 0f;
					return false;
				}
			}
			float romanceChance = initiator.relations.SecondaryRomanceChanceFactor(recipient);
			if (romanceChance < 0.25f) {
				__result = 0f;
				return false;
			}
			int opinionOfTarget = initiator.relations.OpinionOf(recipient);
			if (opinionOfTarget < 5) {
				__result = 0f;
				return false;
			}
			if (recipient.relations.OpinionOf(initiator) < 5) {
				__result = 0f;
				return false;
			}
			float cheatChance = 1f;
			Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, false);
			if (pawn != null) {
				float opinionOfPartner = (float)initiator.relations.OpinionOf(pawn);
				if (initiator.story.traits.HasTrait(RRRTraitDefOf.Polyamorous)) {
					if (SexualityUtilities.HasNonPolyPartner(initiator)) {
						if (initiator.story.traits.HasTrait(RRRTraitDefOf.Philanderer)) {
							if (initiator.Map == pawn.Map) {
								cheatChance = Mathf.InverseLerp(75f, 5f, opinionOfPartner);
							}
							else {
								cheatChance = Mathf.InverseLerp(100f, 50f, opinionOfPartner);
							}
						}
						else {
							cheatChance = Mathf.InverseLerp(50f, -50f, opinionOfPartner);
						}
						if (initiator.story.traits.HasTrait(RRRTraitDefOf.Faithful)) {
							cheatChance = 0f;
						}
					}
					else {
						cheatChance = 1f;
					}
				}
				else {
					if (initiator.story.traits.HasTrait(RRRTraitDefOf.Philanderer)) {
						if (initiator.Map == pawn.Map) {
							cheatChance = Mathf.InverseLerp(75f, 5f, opinionOfPartner);
						}
						else {
							cheatChance = Mathf.InverseLerp(100f, 50f, opinionOfPartner);
						}
					}
					else {
						cheatChance = Mathf.InverseLerp(50f, -50f, opinionOfPartner);
					}
					if (initiator.story.traits.HasTrait(RRRTraitDefOf.Faithful)) {
						cheatChance = 0f;
					}
				}
			}
			float genderAggressiveness;
			string backgroundCulture = SexualityUtilities.GetAdultCulturalAdjective(initiator);
			if (backgroundCulture == "Urbworld") {
				genderAggressiveness = (initiator.gender != Gender.Male ? 0.5f : 1f);
			}
			else if (backgroundCulture == "Imperial") {
				genderAggressiveness = (initiator.gender != Gender.Female ? 0.5f : 1f);
			}
			else if (backgroundCulture == "Tribal") {
				genderAggressiveness = (initiator.gender != Gender.Female ? 0.125f : 1f);
			}
			else if (backgroundCulture == "Medieval") {
				genderAggressiveness = (initiator.gender != Gender.Male ? 0.125f : 1f);
			}
			else {
				genderAggressiveness = 1f;
			}
			float romanceChancePercent = Mathf.InverseLerp(0.25f, 1f, romanceChance);
			float opinionPercent = Mathf.InverseLerp(5f, 100f, (float)opinionOfTarget);
			float orientationMatch = 1f;
			if (initiator.story.traits.HasTrait(RRRTraitDefOf.Asexual) || recipient.story.traits.HasTrait(RRRTraitDefOf.Asexual)) {
				orientationMatch = 0.15f;
			}
			if (initiator.gender != recipient.gender) {
				if (initiator.story.traits.HasTrait(TraitDefOf.Gay) || recipient.story.traits.HasTrait(TraitDefOf.Gay)) {
					orientationMatch = 0.15f;
				}
			}
			if (initiator.gender == recipient.gender) {
				genderAggressiveness = 1f;
				if (initiator.story.traits.HasTrait(RRRTraitDefOf.Straight) || recipient.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
					orientationMatch = 0.15f;
				}
			}
			__result = 1.15f * genderAggressiveness * romanceChancePercent * opinionPercent * cheatChance * orientationMatch;
			return false;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "SuccessChance", null)]
	public static class InteractionWorker_RomanceAttempt_SuccessChance {
		// CHANGE: Updated with new orientation options and traits.
		// CHANGE: Pawns are more likely to rebuff non-ideal partners.
		// CHANGE: Allowed for polyamory.
		public static bool Prefix(Pawn initiator, Pawn recipient, ref float __result) {
			if (!recipient.story.traits.HasTrait(RRRTraitDefOf.Asexual) && !recipient.story.traits.HasTrait(RRRTraitDefOf.Bisexual) && !recipient.story.traits.HasTrait(TraitDefOf.Gay) && !recipient.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
				ExtraTraits.AssignOrientation(recipient);
			}
			float single = 0.6f;
			single *= recipient.relations.SecondaryRomanceChanceFactor(initiator);
			single *= Mathf.InverseLerp(5f, 100f, (float)recipient.relations.OpinionOf(initiator));
			float single1 = 1f;
			if (recipient.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) && !SexualityUtilities.HasNonPolyPartner(recipient)) { }
			else {
				Pawn firstDirectRelationPawn = null;
				if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null) {
					firstDirectRelationPawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
					single1 = 0.6f;
					if (recipient.story.traits.HasTrait(RRRTraitDefOf.Faithful)) {
						single1 = 0f;
					}
				}
				else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null) {
					firstDirectRelationPawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
					single1 = 0.1f;
					if (recipient.story.traits.HasTrait(RRRTraitDefOf.Faithful)) {
						single1 = 0f;
					}
				}
				else if (recipient.GetSpouse() != null && !recipient.GetSpouse().Dead) {
					firstDirectRelationPawn = recipient.GetSpouse();
					single1 = 0.3f;
					if (recipient.story.traits.HasTrait(RRRTraitDefOf.Faithful)) {
						single1 = 0f;
					}
				}
				if (firstDirectRelationPawn != null) {
					single1 *= Mathf.InverseLerp(100f, 0f, (float)recipient.relations.OpinionOf(firstDirectRelationPawn));
					if (recipient.story.traits.HasTrait(RRRTraitDefOf.Philanderer)) {
						single1 *= 1.6f;
						if (firstDirectRelationPawn.Map != recipient.Map) {
							single1 *= 2f;
						}
					}
					single1 *= Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(firstDirectRelationPawn));
				}
			}
			single *= single1;
			__result = Mathf.Clamp01(single);
			if ((initiator.gender == recipient.gender) && (recipient.story.traits.HasTrait(RRRTraitDefOf.Straight))) {
				__result *= 0.6f;
			}
			if ((initiator.gender != recipient.gender) && (recipient.story.traits.HasTrait(TraitDefOf.Gay))) {
				__result *= 0.6f;
			}
			if (recipient.story.traits.HasTrait(RRRTraitDefOf.Asexual)) {
				__result *= 0.3f;
			}
			return false;
		}
	}
	
	[HarmonyPatch(typeof(LovePartnerRelationUtility), "ChangeSpouseRelationsToExSpouse", null)]
	public static class LovePartnerRelationUtility_ChangeSpouseRelationsToExSpouse {
		// CHANGE: Allowed for polyamory.
		public static bool Prefix(Pawn pawn) {
			if (pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous)) {
				IEnumerable<Pawn> spouses = (from p in pawn.relations.RelatedPawns where pawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, p) select p);
				foreach (Pawn spousePawn in spouses) {
					if (!spousePawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous)) {
						pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, spousePawn);
						pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, spousePawn);
					}
				}
				return false;
			}
			return true;
		}
	}
	
	[HarmonyPatch(typeof(LovePartnerRelationUtility), "LovePartnerRelationGenerationChance", null)]
	public static class LovePartnerRelationUtility_LovePartnerRelationGenerationChance {
		// CHANGE: Updated with new orientation options.
		public static bool Prefix(Pawn generated, Pawn other, PawnGenerationRequest request, bool ex, ref float __result) {
			if (generated.ageTracker.AgeBiologicalYearsFloat < 14f) {
				__result = 0f;
				return false;
			}
			if (other.ageTracker.AgeBiologicalYearsFloat < 14f) {
				__result = 0f;
				return false;
			}
			if (generated.gender == other.gender && other.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
				__result = 0f;
				return false;
			}
			if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay)) {
				__result = 0f;
				return false;
			}
			float single = 1f;
			if (ex) {
				int num = 0;
				List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
				for (int i = 0; i < directRelations.Count; i++) {
					if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def)) {
						num++;
					}
				}
				single = Mathf.Pow(0.2f, (float)num);
			}
			else if (LovePartnerRelationUtility.HasAnyLovePartner(other)) {
				__result = 0f;
				return false;
			}
			float single1 = (generated.gender != other.gender ? 1f : 0.01f);
			float generationChanceAgeFactor = GetGenerationChanceAgeFactor(generated);
			float generationChanceAgeFactor1 = GetGenerationChanceAgeFactor(other);
			float generationChanceAgeGapFactor = GetGenerationChanceAgeGapFactor(generated, other, ex);
			float single2 = 1f;
			if (generated.GetRelations(other).Any<PawnRelationDef>((PawnRelationDef x) => x.familyByBloodRelation)) {
				single2 = 0.01f;
			}
			float melaninCommonalityFactor = 1f;
			if (!request.FixedMelanin.HasValue) {
				melaninCommonalityFactor = PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin);
			}
			else {
				float? fixedMelanin = request.FixedMelanin;
				melaninCommonalityFactor = ChildRelationUtility.GetMelaninSimilarityFactor(fixedMelanin.Value, other.story.melanin);
			}
			__result = single * generationChanceAgeFactor * generationChanceAgeFactor1 * generationChanceAgeGapFactor * single1 * melaninCommonalityFactor * single2;
			return false;
		}
		private static float GetGenerationChanceAgeFactor(Pawn p) {
			float single = GenMath.LerpDouble(14f, 27f, 0f, 1f, p.ageTracker.AgeBiologicalYearsFloat);
			return Mathf.Clamp(single, 0f, 1f);
		}
		private static float GetGenerationChanceAgeGapFactor(Pawn p1, Pawn p2, bool ex) {
			float single = Mathf.Abs(p1.ageTracker.AgeBiologicalYearsFloat - p2.ageTracker.AgeBiologicalYearsFloat);
			if (ex) {
				float generateAsLovers = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p1, p2);
				if (generateAsLovers >= 0f) {
					single = Mathf.Min(single, generateAsLovers);
				}
				float generateAsLovers1 = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(p2, p1);
				if (generateAsLovers1 >= 0f) {
					single = Mathf.Min(single, generateAsLovers1);
				}
			}
			if (single > 40f) {
				return 0f;
			}
			float single1 = GenMath.LerpDouble(0f, 20f, 1f, 0.001f, single);
			return Mathf.Clamp(single1, 0.001f, 1f);
		}
		private static float MinPossibleAgeGapAtMinAgeToGenerateAsLovers(Pawn p1, Pawn p2) {
			float ageChronologicalYearsFloat = p1.ageTracker.AgeChronologicalYearsFloat - 14f;
			if (ageChronologicalYearsFloat < 0f) {
				Log.Warning("at < 0", false);
				return 0f;
			}
			float single = PawnRelationUtility.MaxPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, p2.ageTracker.AgeChronologicalYearsFloat, ageChronologicalYearsFloat);
			float single1 = PawnRelationUtility.MinPossibleBioAgeAt(p2.ageTracker.AgeBiologicalYearsFloat, ageChronologicalYearsFloat);
			if (single < 0f) {
				return -1f;
			}
			if (single < 14f) {
				return -1f;
			}
			if (single1 <= 14f) {
				return 0f;
			}
			return single1 - 14f;
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), "GenerateTraits", null)]
	public static class PawnGenerator_GenerateTraits {
		// CHANGE: Add orientation trait after other traits are selected.
		public static void Postfix(Pawn pawn) {
			if (pawn.story.traits.HasTrait(RRRTraitDefOf.Asexual) || pawn.story.traits.HasTrait(RRRTraitDefOf.Bisexual) || pawn.story.traits.HasTrait(TraitDefOf.Gay) || pawn.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
				return;
			}
			ExtraTraits.AssignOrientation(pawn);
			return;
		}
	}
	
	//
	// PawnRelationWorker_Child "CreateRelation"
	//
	
	//
	// PawnRelationWorker_Sibling "CreateRelation"
	//
	
	//
	// PawnRelationWorker_Sibling "GenerateParent"	
	//
	
	[HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryLovinChanceFactor", null)]
	public static class Pawn_RelationsTracker_SecondaryLovinChanceFactor {
		// CHANGE: Updated with new orientation options.
		// CHANGE: Gender age preferences are now the same, except for mild cultural variation.
		// CHANGE: Pawns with Ugly trait are less uninterested romantically in other ugly pawns.
		internal static FieldInfo _pawn;
		public static bool Prefix(Pawn otherPawn, ref float __result, ref Pawn_RelationsTracker __instance) {
			Pawn pawn = __instance.GetPawn();
			if (pawn == otherPawn) {
				__result = 0f;
				return false;
			}
			if ((!pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike) && pawn.def != otherPawn.def) {
				__result = 0f;
				return false;
			}
			float crossSpecies = 1;
			if (pawn.def != otherPawn.def) {
				crossSpecies = Controller.Settings.alienLoveChance/100;
			}
			if (Rand.ValueSeeded(pawn.thingIDNumber ^ 3273711) >= 0.015f) {
				if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(RRRTraitDefOf.Asexual)) {
					__result = 0f;
					return false;
				}
				if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay)) {
					if (otherPawn.gender != pawn.gender) {
						__result = 0f;
						return false;
					}
				}
				if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(RRRTraitDefOf.Straight)) {
					if (otherPawn.gender == pawn.gender) {
						__result = 0f;
						return false;
					}
				}
			}
			float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
			float targetAge = otherPawn.ageTracker.AgeBiologicalYearsFloat;
			float targetAgeLikelihood = 1f;
			if (targetAge < 16f) {
				__result = 0f;
				return false;
			}
			float youngestTargetAge = Mathf.Max(16f, ageBiologicalYearsFloat - 30f);
			float youngestReasonableTargetAge = Mathf.Max(20f, ageBiologicalYearsFloat, ageBiologicalYearsFloat - 10f);
			targetAgeLikelihood = GenMath.FlatHill(0.15f, youngestTargetAge, youngestReasonableTargetAge, ageBiologicalYearsFloat + 7f, ageBiologicalYearsFloat + 30f, 0.15f, targetAge);
			float targetBaseCapabilities = 1f;
			targetBaseCapabilities *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
			targetBaseCapabilities *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
			targetBaseCapabilities *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
			int initiatorBeauty = 0;
			int targetBeauty = 0;
			if (otherPawn.RaceProps.Humanlike) {
				initiatorBeauty = pawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
			}
			if (otherPawn.RaceProps.Humanlike) {
				targetBeauty = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
			}
			float targetBeautyMod = 1f;
			if (targetBeauty == -2) {
				targetBeautyMod = (initiatorBeauty >= 0 ? 0.3f : 0.8f);;
			}
			if (targetBeauty == -1) {
				targetBeautyMod = (initiatorBeauty >= 0 ? 0.75f : 0.9f);
			}
			if (targetBeauty == 1) {
				targetBeautyMod = 1.7f;
			}
			else if (targetBeauty == 2) {
				targetBeautyMod = 2.3f;
			}
			string backgroundCulture = SexualityUtilities.GetAdultCulturalAdjective(pawn);
			float ageDiffPref = 1f;
			if (backgroundCulture == "Urbworld" || backgroundCulture == "Medieval") {
				if (pawn.gender == Gender.Male && otherPawn.gender == Gender.Female) {
					ageDiffPref = (ageBiologicalYearsFloat <= targetAge ? 0.8f : 1.2f);
				}
				else if (pawn.gender == Gender.Female && otherPawn.gender == Gender.Male) {
					ageDiffPref = (ageBiologicalYearsFloat <= targetAge ? 1.2f : 0.8f);
				}
			}
			if (backgroundCulture == "Tribal" || backgroundCulture == "Imperial") {
				if (pawn.gender == Gender.Male && otherPawn.gender == Gender.Female) {
					ageDiffPref = (ageBiologicalYearsFloat <= targetAge ? 1.2f : 0.8f);
				}
				else if (pawn.gender == Gender.Female && otherPawn.gender == Gender.Male) {
					ageDiffPref = (ageBiologicalYearsFloat <= targetAge ? 0.8f : 1.2f);
				}
			}
			float initiatorYoung = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
			float targetYoung = Mathf.InverseLerp(15f, 18f, targetAge);
			__result = targetAgeLikelihood * ageDiffPref * targetBaseCapabilities * initiatorYoung * targetYoung * targetBeautyMod * crossSpecies;
			return false;
		}
		private static Pawn GetPawn(this Pawn_RelationsTracker _this) {
			bool flag = Pawn_RelationsTracker_SecondaryLovinChanceFactor._pawn == null;
			if (flag) {
				Pawn_RelationsTracker_SecondaryLovinChanceFactor._pawn = typeof(Pawn_RelationsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
				bool flag2 = Pawn_RelationsTracker_SecondaryLovinChanceFactor._pawn == null;
				if (flag2) {
					Log.ErrorOnce("Unable to reflect Pawn_RelationsTracker.pawn!", 305432421);
				}
			}
			return (Pawn)Pawn_RelationsTracker_SecondaryLovinChanceFactor._pawn.GetValue(_this);
		}
	}

	[HarmonyPatch(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover), "CurrentStateInternal")]
	public static class ThoughtWorker_WantToSleepWithSpouseOrLover_CurrentStateInternal {
		// CHANGE: Allowed for polyamory.
		public static void CurrentStateInternal(ref ThoughtState __result, Pawn p) {
			if (__result.StageIndex != ThoughtState.Inactive.StageIndex) {
				DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false);
				bool multiplePartners = (from r in p.relations.PotentiallyRelatedPawns where LovePartnerRelationUtility.LovePartnerRelationExists(p, r) select r).Count() > 1;
				bool partnerBedInRoom = (from t in p.ownership.OwnedBed.GetRoom().ContainedBeds where t.OwnersForReading.Contains(directPawnRelation.otherPawn) select t).Count() > 0;
				if (directPawnRelation != null && p.ownership.OwnedBed != null && p.story.traits.HasTrait(RRRTraitDefOf.Polyamorous) && multiplePartners && partnerBedInRoom) {
					__result = false;
				}
			}
		}
	}

	public class Thought_WantToSleepWithSpouseOrLoverRRR : Thought_WantToSleepWithSpouseOrLover {
		public override string LabelCap {
			get {
				if(this.pawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous)) {
					return string.Format(base.CurStage.label, "my partners").CapitalizeFirst();
				}
				DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(this.pawn, false);
				return string.Format(base.CurStage.label, directPawnRelation.otherPawn.LabelShort).CapitalizeFirst();
			}
		}
	}

//
// NEW CODE
//

	public class InteractionWorker_NullWorker : InteractionWorker {
		public InteractionWorker_NullWorker() { }
		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets) {
			letterLabel = null;
			letterText = null;
			letterDef = null;
            lookTargets = null;
		}
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient) {
			return 0f;
		}
	}	

	public class JobDriver_DoLovinCasual : JobDriver {
		public override bool TryMakePreToilReservations(bool errorOnFailed) {
			return true;
		}
		private const int TicksBetweenHeartMotes = 100;
		private const int duration = 20;
		private int ticksLeft = 20;
		private TargetIndex PartnerInd = TargetIndex.A;
		private TargetIndex BedInd = TargetIndex.B;
		private TargetIndex SlotInd = TargetIndex.C;
		private Building_Bed Bed {
			get { return (Building_Bed)((Thing)base.job.GetTarget(this.BedInd)); }
		}
		private Pawn actor {
			get { return base.GetActor(); }
		}
		private Pawn Partner {
			get { return (Pawn)((Thing)base.job.GetTarget(this.PartnerInd)); }
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksLeft, "ticksLeft", 0, false);
		}
		private IntVec3 GetSleepingSpot(Building_Bed bed) {
			for (int i = 0; i < bed.SleepingSlotsCount; i++) {
				if (bed.GetCurOccupant(i) == null) {
					return bed.GetSleepingSlotPos(i);
				}
			}
			return bed.GetSleepingSlotPos(0);
		}
		private bool IsInOrByBed(Building_Bed b, Pawn p) {
			for (int i = 0; i < b.SleepingSlotsCount; i++) {
				if (b.GetSleepingSlotPos(i).InHorDistOf(p.Position, 1f)) {
					return true;
				}
			}
			return false;
		}
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils() {
			yield return Toils_Reserve.Reserve(this.BedInd, 2, 0, null);
			yield return Toils_Goto.Goto(this.SlotInd, PathEndMode.OnCell);
			yield return new Toil {
				initAction = delegate {
					this.ticksLeftThisToil = 300;
				},
				tickAction = delegate {
					if (this.IsInOrByBed(this.Bed, this.Partner)) {
						this.ticksLeftThisToil = 0;
					}
				},
				defaultCompleteMode = ToilCompleteMode.Delay
			};
			Toil layDown = new Toil();
			layDown.initAction = delegate {
				layDown.actor.pather.StopDead();
				JobDriver curDriver = layDown.actor.jobs.curDriver;
//				curDriver.layingDown = 2;
				curDriver.asleep = false;
			};
			layDown.tickAction = delegate {
				PawnUtility.GainComfortFromCellIfPossible(this.actor);
			};
			yield return layDown;
			Toil loveToil = new Toil();
			loveToil.initAction = delegate {
				this.ticksLeftThisToil = 1200;
				if (LovePartnerRelationUtility.HasAnyLovePartner(this.actor)) {
					Pawn pawn = LovePartnerRelationUtility.ExistingLovePartner(this.actor);
					if (this.Partner != pawn) {
						if (!pawn.Dead) {
							if (pawn.Map == this.actor.Map || (double)Rand.Value < 0.25) {
								pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, this.actor);
							}
						}
					}
				}
			};
			loveToil.tickAction = delegate {
				if (this.ticksLeftThisToil % 100 == 0) {
					MoteMaker.ThrowMetaIcon(this.actor.Position, this.actor.Map, ThingDefOf.Mote_Heart);
				}
				if (this.ticksLeftThisToil % 100 == 0) {
					this.actor.needs.joy.GainJoy(0.005f, RRRMiscDefOf.Lewd);
				}
			};
			loveToil.defaultCompleteMode = ToilCompleteMode.Delay;
			loveToil.AddFailCondition(() => this.Partner.Dead || (this.ticksLeftThisToil > 100 && !this.IsInOrByBed(this.Bed, this.Partner)));
			yield return loveToil;
			yield return new Toil {
				initAction = delegate {
					this.actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.GotSomeLovin, this.Partner);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield break;
		}
	}

	public class JobDriver_JobDateFollow : JobDriver {
		public override bool TryMakePreToilReservations(bool errorOnFailed) {
			return true;
		}
		private TargetIndex PartnerInd = TargetIndex.A;
		private Pawn actor {
			get { return base.GetActor(); }
		}
		private Pawn Partner {
			get { return (Pawn)((Thing)base.job.GetTarget(this.PartnerInd)); }
		}
		public override RandomSocialMode DesiredSocialMode() {
			return RandomSocialMode.SuperActive;
		}
		//private bool IsPartnerNearby() {
		//	return this.actor.Position.InHorDistOf(this.Partner.Position, 2f);
		//}
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils() {
			Toil FollowPartner = new Toil();
			FollowPartner.defaultCompleteMode = ToilCompleteMode.Delay;
			FollowPartner.AddFailCondition(() => !this.Partner.Spawned);
			FollowPartner.AddFailCondition(() => this.Partner.Dead);
			FollowPartner.AddFailCondition(() => this.Partner.CurJob.def != RRRJobDefOf.JobDateLead);
			FollowPartner.initAction = delegate {
				this.ticksLeftThisToil = 200;
				this.actor.pather.StartPath(this.Partner, PathEndMode.Touch);
			};
			FollowPartner.tickAction = delegate {
				this.actor.needs.joy.GainJoy(0.0001f, RRRMiscDefOf.Social);
			};
			for (int i = 0; i < 100; i++) {
				yield return FollowPartner;
			}
			yield break;
		}
	}

	public class JobDriver_JobDateLead : JobDriver {
		public override bool TryMakePreToilReservations(bool errorOnFailed) {
			return true;
		}
		private const int duration = 20;
		private int ticksLeft = 20;
		private TargetIndex PartnerInd = TargetIndex.A;
		private Pawn actor {
			get { return base.GetActor(); }
		}
		private Pawn Partner {
			get { return (Pawn)((Thing)base.job.GetTarget(this.PartnerInd)); }
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksLeft, "ticksLeft", 0, false);
		}
		public override RandomSocialMode DesiredSocialMode() {
			return RandomSocialMode.SuperActive;
		}
		//private bool IsPartnerNearby() {
		//	return this.actor.Position.InHorDistOf(this.Partner.Position, 2f);
		//}
		public Toil GotoCell(LocalTargetInfo target) {
			Toil toil = new Toil();
			toil.initAction = delegate {
				Pawn actor = toil.actor;
				actor.pather.StartPath(target, PathEndMode.OnCell);
			};
			toil.tickAction = delegate {
				this.actor.needs.joy.GainJoy(0.0001f, RRRMiscDefOf.Social);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.AddFailCondition(() => !this.Partner.Spawned);
			toil.AddFailCondition(() => this.Partner.Dead);
			toil.AddFailCondition(() => this.Partner.CurJob.def != RRRJobDefOf.JobDateFollow);
			return toil;
		}
		private Toil WaitForPartner() {
			Toil toil = new Toil();
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.initAction = delegate {
				this.ticksLeftThisToil = 700;
			};
			toil.tickAction = delegate {
				this.actor.needs.joy.GainJoy(0.0001f, RRRMiscDefOf.Social);
			};
			toil.AddFailCondition(() => PawnUtility.WillSoonHaveBasicNeed(base.GetActor()) || PawnUtility.WillSoonHaveBasicNeed(this.Partner));
			toil.AddFailCondition(() => !this.Partner.Spawned);
			toil.AddFailCondition(() => this.Partner.Dead);
			toil.AddFailCondition(() => this.Partner.CurJob.def != RRRJobDefOf.JobDateFollow);
			return toil;
		}
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils() {
			foreach (LocalTargetInfo target in base.job.targetQueueB) {
				yield return this.GotoCell(target.Cell);
				yield return this.WaitForPartner();
			}
			yield break;
		}
	}

	public class JobDriver_LeadHookup : JobDriver {
		public override bool TryMakePreToilReservations(bool errorOnFailed) {
			return true;
		}
		public bool successfulPass = true;
		public bool wasSuccessfulPass {
			get { return this.successfulPass; }
		}
		private Pawn actor {
			get { return base.GetActor(); }
		}
		private Pawn TargetPawn {
			get { return base.TargetThingA as Pawn; }
		}
		private Building_Bed TargetBed {
			get { return base.TargetThingB as Building_Bed; }
		}
		private TargetIndex TargetPawnIndex {
			get { return TargetIndex.A; }
		}
		private TargetIndex TargetBedIndex {
			get { return TargetIndex.B; }
		}
		private bool DoesTargetPawnAcceptAdvance() {
			return !PawnUtility.WillSoonHaveBasicNeed(this.TargetPawn) && !PawnUtility.EnemiesAreNearby(this.TargetPawn, 9, false) && this.TargetPawn.CurJob.def != JobDefOf.LayDown && this.TargetPawn.CurJob.def != JobDefOf.BeatFire && this.TargetPawn.CurJob.def != JobDefOf.Arrest && this.TargetPawn.CurJob.def != JobDefOf.Capture && this.TargetPawn.CurJob.def != JobDefOf.EscortPrisonerToBed && this.TargetPawn.CurJob.def != JobDefOf.ExtinguishSelf && this.TargetPawn.CurJob.def != JobDefOf.FleeAndCower && this.TargetPawn.CurJob.def != JobDefOf.MarryAdjacentPawn && this.TargetPawn.CurJob.def != JobDefOf.PrisonerExecution && this.TargetPawn.CurJob.def != JobDefOf.ReleasePrisoner && this.TargetPawn.CurJob.def != JobDefOf.Rescue && this.TargetPawn.CurJob.def != JobDefOf.SocialFight && this.TargetPawn.CurJob.def != JobDefOf.SpectateCeremony && this.TargetPawn.CurJob.def != JobDefOf.TakeToBedToOperate && this.TargetPawn.CurJob.def != JobDefOf.TakeWoundedPrisonerToBed && this.TargetPawn.CurJob.def != JobDefOf.UseCommsConsole && this.TargetPawn.CurJob.def != JobDefOf.Vomit && this.TargetPawn.CurJob.def != JobDefOf.Wait_Downed && SexualityUtilities.WillPawnTryHookup(this.TargetPawn) && SexualityUtilities.IsHookupAppealing(this.TargetPawn, base.GetActor());
		}
		private bool IsTargetPawnOkay() {
			return !this.TargetPawn.Dead && !this.TargetPawn.Downed;
		}
		private bool IsTargetPawnFreeForHookup() {
			return !PawnUtility.WillSoonHaveBasicNeed(this.TargetPawn) && !PawnUtility.EnemiesAreNearby(this.TargetPawn, 9, false) && this.TargetPawn.CurJob.def != JobDefOf.LayDown && this.TargetPawn.CurJob.def != JobDefOf.BeatFire && this.TargetPawn.CurJob.def != JobDefOf.Arrest && this.TargetPawn.CurJob.def != JobDefOf.Capture && this.TargetPawn.CurJob.def != JobDefOf.EscortPrisonerToBed && this.TargetPawn.CurJob.def != JobDefOf.ExtinguishSelf && this.TargetPawn.CurJob.def != JobDefOf.FleeAndCower && this.TargetPawn.CurJob.def != JobDefOf.MarryAdjacentPawn && this.TargetPawn.CurJob.def != JobDefOf.PrisonerExecution && this.TargetPawn.CurJob.def != JobDefOf.ReleasePrisoner && this.TargetPawn.CurJob.def != JobDefOf.Rescue && this.TargetPawn.CurJob.def != JobDefOf.SocialFight && this.TargetPawn.CurJob.def != JobDefOf.SpectateCeremony && this.TargetPawn.CurJob.def != JobDefOf.TakeToBedToOperate && this.TargetPawn.CurJob.def != JobDefOf.TakeWoundedPrisonerToBed && this.TargetPawn.CurJob.def != JobDefOf.UseCommsConsole && this.TargetPawn.CurJob.def != JobDefOf.Vomit && this.TargetPawn.CurJob.def != JobDefOf.Wait_Downed;
		}
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils() {
			if (this.IsTargetPawnFreeForHookup()) {
				yield return Toils_Goto.GotoThing(this.TargetPawnIndex, PathEndMode.Touch);
				Toil TryItOn = new Toil();
				TryItOn.AddFailCondition(() => !this.IsTargetPawnOkay());
				TryItOn.defaultCompleteMode = ToilCompleteMode.Delay;
				TryItOn.initAction = delegate {
					this.ticksLeftThisToil = 50;
					MoteMaker.ThrowMetaIcon(this.GetActor().Position, this.GetActor().Map, ThingDefOf.Mote_Heart);
				};
				yield return TryItOn;
				Toil AwaitResponse = new Toil();
				AwaitResponse.defaultCompleteMode = ToilCompleteMode.Instant;
				AwaitResponse.initAction = delegate {
					List<RulePackDef> list = new List<RulePackDef>();
					this.successfulPass = this.DoesTargetPawnAcceptAdvance();
					if (this.successfulPass) {
						MoteMaker.ThrowMetaIcon(this.TargetPawn.Position, this.TargetPawn.Map, ThingDefOf.Mote_Heart);
						list.Add(RRRMiscDefOf.HookupSucceeded);
					}
					else {
						MoteMaker.ThrowMetaIcon(this.TargetPawn.Position, this.TargetPawn.Map, ThingDefOf.Mote_IncapIcon);
						this.GetActor().needs.mood.thoughts.memories.TryGainMemory(RRRThoughtDefOf.RebuffedMyHookupAttempt, this.TargetPawn);
						this.TargetPawn.needs.mood.thoughts.memories.TryGainMemory(RRRThoughtDefOf.FailedHookupAttemptOnMe, this.GetActor());
						list.Add(RRRMiscDefOf.HookupFailed);
					}
					Find.PlayLog.Add(new PlayLogEntry_Interaction(RRRMiscDefOf.TriedHookupWith, this.pawn, this.TargetPawn, list));
				};
				AwaitResponse.AddFailCondition(() => !this.wasSuccessfulPass);
				yield return AwaitResponse;
				if (this.wasSuccessfulPass) {
					yield return new Toil {
						defaultCompleteMode = ToilCompleteMode.Instant,
						initAction = delegate {
							if (this.wasSuccessfulPass) {
								this.GetActor().jobs.jobQueue.EnqueueFirst(new Job(RRRJobDefOf.DoLovinCasual, this.TargetPawn, this.TargetBed, this.TargetBed.GetSleepingSlotPos(0)), null);
								this.TargetPawn.jobs.jobQueue.EnqueueFirst(new Job(RRRJobDefOf.DoLovinCasual, this.GetActor(), this.TargetBed, this.TargetBed.GetSleepingSlotPos(1)), null);
								this.GetActor().jobs.EndCurrentJob(JobCondition.InterruptOptional, true);
								this.TargetPawn.jobs.EndCurrentJob(JobCondition.InterruptOptional, true);
							}
						}
					};
				}
				yield break;
			}
		}
	}

	public class JobDriver_ProposeDate : JobDriver {
		public override bool TryMakePreToilReservations(bool errorOnFailed) {
			return true;
		}
		public bool successfulPass = true;
		private Pawn TargetPawn {
			get { return base.TargetThingA as Pawn; }
		}
		private Building_Bed TargetBed {
			get { return base.TargetThingB as Building_Bed; }
		}
		private TargetIndex TargetPawnIndex {
			get { return TargetIndex.A; }
		}
		private TargetIndex TargetBedIndex {
			get { return TargetIndex.B; }
		}
		private bool IsTargetPawnFreeForDate() {
			return !PawnUtility.WillSoonHaveBasicNeed(this.TargetPawn) && !PawnUtility.EnemiesAreNearby(this.TargetPawn, 9, false) && this.TargetPawn.CurJob.def != JobDefOf.LayDown && this.TargetPawn.CurJob.def != JobDefOf.BeatFire && this.TargetPawn.CurJob.def != JobDefOf.Arrest && this.TargetPawn.CurJob.def != JobDefOf.Capture && this.TargetPawn.CurJob.def != JobDefOf.EscortPrisonerToBed && this.TargetPawn.CurJob.def != JobDefOf.ExtinguishSelf && this.TargetPawn.CurJob.def != JobDefOf.FleeAndCower && this.TargetPawn.CurJob.def != JobDefOf.MarryAdjacentPawn && this.TargetPawn.CurJob.def != JobDefOf.PrisonerExecution && this.TargetPawn.CurJob.def != JobDefOf.ReleasePrisoner && this.TargetPawn.CurJob.def != JobDefOf.Rescue && this.TargetPawn.CurJob.def != JobDefOf.SocialFight && this.TargetPawn.CurJob.def != JobDefOf.SpectateCeremony && this.TargetPawn.CurJob.def != JobDefOf.TakeToBedToOperate && this.TargetPawn.CurJob.def != JobDefOf.TakeWoundedPrisonerToBed && this.TargetPawn.CurJob.def != JobDefOf.UseCommsConsole && this.TargetPawn.CurJob.def != JobDefOf.Vomit && this.TargetPawn.CurJob.def != JobDefOf.Wait_Downed;
		}
		private bool TryFindUnforbiddenDatePath(Pawn p1, Pawn p2, IntVec3 root, out List<IntVec3> result) {
			int StartRadialIndex = GenRadial.NumCellsInRadius(14f);
			int EndRadialIndex = GenRadial.NumCellsInRadius(2f);
			int RadialIndexStride = 3;
			float single;
			List<IntVec3> intVec3s = new List<IntVec3>() { root };
			IntVec3 intVec3 = root;
			for (int i = 0; i < 8; i++) {
				IntVec3 invalid = IntVec3.Invalid;
				float single1 = -1f;
				for (int j = StartRadialIndex; j > EndRadialIndex; j -= RadialIndexStride) {
					IntVec3 radialPattern = intVec3 + GenRadial.RadialPattern[j];
					if (radialPattern.InBounds(p1.Map) && radialPattern.Standable(p1.Map) && !radialPattern.IsForbidden(p1) && !radialPattern.IsForbidden(p2) && !radialPattern.GetTerrain(p1.Map).avoidWander && GenSight.LineOfSight(intVec3, radialPattern, p1.Map, false, null, 0, 0) && !radialPattern.Roofed(p1.Map) && !PawnUtility.KnownDangerAt(radialPattern, p1.Map, p1) && !PawnUtility.KnownDangerAt(radialPattern, p1.Map, p2)) {
						float lengthManhattan = 10000f;
						for (int k = 0; k < intVec3s.Count; k++) {
							lengthManhattan += (float)(intVec3s[k] - radialPattern).LengthManhattan;
						}
						float lengthManhattan1 = (float)(radialPattern - root).LengthManhattan;
						if (lengthManhattan1 > 40f) {
							lengthManhattan *= Mathf.InverseLerp(70f, 40f, lengthManhattan1);
						}
						if (intVec3s.Count >= 2) {
							IntVec3 item = intVec3s[intVec3s.Count - 1] - intVec3s[intVec3s.Count - 2];
							float angleFlat = item.AngleFlat;
							float angleFlat1 = (radialPattern - intVec3).AngleFlat;
							if (angleFlat1 <= angleFlat) {
								angleFlat -= 360f;
								single = angleFlat1 - angleFlat;
							}
							else {
								single = angleFlat1 - angleFlat;
							}
							if (single > 110f) {
								lengthManhattan *= 0.01f;
							}
						}
						if (intVec3s.Count >= 4 && (intVec3 - root).LengthManhattan < (radialPattern - root).LengthManhattan) {
							lengthManhattan *= 1E-05f;
						}
						if (lengthManhattan > single1) {
							invalid = radialPattern;
							single1 = lengthManhattan;
						}
					}
				}
				if (single1 < 0f) {
					result = null;
					return false;
				}
				intVec3s.Add(invalid);
				intVec3 = invalid;
			}
			intVec3s.Add(root);
			result = intVec3s;
			return true;
		}
		private bool IsTargetPawnOkay() {
			return !this.TargetPawn.Dead && !this.TargetPawn.Downed;
		}
		private bool TryFindMostBeautifulRootInDistance(int distance, Pawn p1, Pawn p2, out IntVec3 best) {
			best = default(IntVec3);
			List<IntVec3> list = new List<IntVec3>();
			for (int i = 0; i < 200; i++) {
				IntVec3 item;
				if (CellFinder.TryFindRandomCellNear(p1.Position, p1.Map, distance, (IntVec3 c) => GenGrid.InBounds(c, p1.Map) && !ForbidUtility.IsForbidden(c, p1) && !ForbidUtility.IsForbidden(c, p2) && ReachabilityUtility.CanReach(p1, c, PathEndMode.OnCell, Danger.Some, false, 0), out item)) {
					list.Add(item);
				}
			}
			bool result;
			if (list.Count == 0) {
				result = false;
				Log.Message("No date walk destination found.");
			}
			else {
				List<IntVec3> list2 = (from c in list
				orderby BeautyUtility.AverageBeautyPerceptible(c, p1.Map) descending
				select c).ToList<IntVec3>();
				best = list2.FirstOrDefault<IntVec3>();
				list2.Reverse();
				Log.Message("Date walk destinations found from beauty "+BeautyUtility.AverageBeautyPerceptible(best, p1.Map)+" to "+BeautyUtility.AverageBeautyPerceptible(list2.FirstOrDefault<IntVec3>(), p1.Map));
				result = true;
			}
			return result;
		}
		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils() {
			if (this.IsTargetPawnFreeForDate()) {
				yield return Toils_Goto.GotoThing(this.TargetPawnIndex, PathEndMode.Touch);
				Toil AskOut = new Toil();
				AskOut.AddFailCondition(() => !this.IsTargetPawnOkay());
				AskOut.defaultCompleteMode = ToilCompleteMode.Delay;
				AskOut.initAction = delegate {
					this.ticksLeftThisToil = 50;
					MoteMaker.ThrowMetaIcon(this.GetActor().Position, this.GetActor().Map, ThingDefOf.Mote_Heart);
				};
				yield return AskOut;
				Toil AwaitResponse = new Toil();
				AwaitResponse.defaultCompleteMode = ToilCompleteMode.Instant;
				AwaitResponse.initAction = delegate {
					this.successfulPass = this.IsTargetPawnFreeForDate();
					if (this.successfulPass) {
						MoteMaker.ThrowMetaIcon(this.TargetPawn.Position, this.TargetPawn.Map, ThingDefOf.Mote_Heart);
					}
					else {
						MoteMaker.ThrowMetaIcon(this.TargetPawn.Position, this.TargetPawn.Map, ThingDefOf.Mote_IncapIcon);
					}
				};
				AwaitResponse.AddFailCondition(() => !this.successfulPass);
				yield return AwaitResponse;
				if (this.successfulPass) {
					yield return new Toil {
						defaultCompleteMode = ToilCompleteMode.Instant,
						initAction = delegate {
							Job job = new Job(RRRJobDefOf.JobDateLead);
							IntVec3 root;
							if (this.TryFindMostBeautifulRootInDistance(40, this.pawn, this.TargetPawn, out root)) {
								List<IntVec3> list = null;
								if (this.TryFindUnforbiddenDatePath(this.pawn, this.TargetPawn, root, out list)) {
									Log.Message("Date walk path found.");
									job.targetQueueB = new List<LocalTargetInfo>();
									for (int i = 1; i < list.Count; i++) {
										job.targetQueueB.Add(list[i]);
									}
									job.locomotionUrgency = LocomotionUrgency.Amble;
									job.targetA = this.TargetPawn;
									this.GetActor().jobs.jobQueue.EnqueueFirst(job, null);
									Job job2 = new Job(RRRJobDefOf.JobDateFollow);
									job2.locomotionUrgency = LocomotionUrgency.Amble;
									job2.targetA = this.GetActor();
									this.TargetPawn.jobs.jobQueue.EnqueueFirst(job2, null);
									this.GetActor().jobs.EndCurrentJob(JobCondition.InterruptOptional, true);
									this.TargetPawn.jobs.EndCurrentJob(JobCondition.InterruptOptional, true);
								}
								else {
									Log.Message("No date walk path found.");
								}
							}
						}
					};
				}
				yield break;
			}
		}
	}

	public class JoyGiver_CasualHookup : JoyGiver {
		public static float percentRate = Controller.Settings.hookupRate / 2;
		public override Job TryGiveJob(Pawn pawn) {
			Job result;
			if (!InteractionUtility.CanInitiateInteraction(pawn)) {
				result = null;
			}
			else if (!SexualityUtilities.WillPawnTryHookup(pawn)) {
				result = null;
			}
			else if (PawnUtility.WillSoonHaveBasicNeed(pawn)) {
				result = null;
			}
			else {
				Pawn pawn2 = SexualityUtilities.FindAttractivePawn(pawn);
				if (pawn2 == null) {
					result = null;
				}
				else {
					Building_Bed building_Bed = SexualityUtilities.FindHookupBed(pawn, pawn2);
					if (building_Bed == null) {
						result = null;
					}
					else if (100f * Rand.Value > JoyGiver_CasualHookup.percentRate) {
						result = null;
					}
					else {
						result = new Job(this.def.jobDef, pawn2, building_Bed);
					}
				}
			}
			return result;
		}
	}
	
	public class JoyGiver_Date : JoyGiver {
		public static float percentRate = Controller.Settings.dateRate / 2;
		public override Job TryGiveJob(Pawn pawn) {
			Job result;
			if (!InteractionUtility.CanInitiateInteraction(pawn)) {
				result = null;
			}
			else if (!LovePartnerRelationUtility.HasAnyLovePartner(pawn)) {
				result = null;
			}
			else {
				Pawn pawn2 = LovePartnerRelationUtility.ExistingLovePartner(pawn);
				if (!pawn2.Spawned) {
					result = null;
				}
				else if (!RestUtility.Awake(pawn2)) {
					result = null;
				}
				else if (!JoyUtility.EnjoyableOutsideNow(pawn, null)) {
					result = null;
				}
				else if (PawnUtility.WillSoonHaveBasicNeed(pawn)) {
					result = null;
				}
				else if (100f * Rand.Value > JoyGiver_Date.percentRate) {
					result = null;
				}
				else {
					result = new Job(this.def.jobDef, pawn2);
				}
			}
			return result;
		}
	}		

	public static class SexualityUtilities {
		public static Pawn FindAttractivePawn(Pawn p1) {
			Pawn result;
			if (p1.story.traits.HasTrait(RRRTraitDefOf.Asexual)) {
				result = null;
			}
			else {
				IEnumerable<Pawn> enumerable = p1.Map.mapPawns.FreeColonistsSpawned;
				enumerable = enumerable.Except(from p in enumerable
				where (p.story.traits.HasTrait(RRRTraitDefOf.Asexual) || !p.RaceProps.Humanlike || (p.story.traits.HasTrait(TraitDefOf.Gay) && p.gender != p1.gender) || (p.story.traits.HasTrait(RRRTraitDefOf.Straight) && p.gender == p1.gender)) && (double)Rand.Value < 0.8
				select p);
				enumerable = from p in enumerable
				where p.Map == p1.Map && p.Faction == p1.Faction
				select p;
				if (enumerable.Count<Pawn>() == 0) {
					result = null;
				}
				else {
					Pawn pawn = null;
					GenCollection.TryRandomElementByWeight<Pawn>(enumerable, (Pawn x) => p1.relations.SecondaryRomanceChanceFactor(x) * p1.relations.SecondaryRomanceChanceFactor(x), out pawn);
					if (pawn == null) {
						result = null;
					}
					else if (pawn == p1) {
						result = null;
					}
					else if (LovePartnerRelationUtility.HasAnyLovePartner(pawn) && Rand.Value < 0.85f) {
						result = null;
					}
					else if (pawn == LovePartnerRelationUtility.ExistingLovePartner(p1)) {
						result = null;
					}
					else if ((double)p1.relations.SecondaryRomanceChanceFactor(pawn) < 0.05) {
						result = null;
					}
					else {
						result = pawn;
					}
				}
			}
			return result;
		}
		public static Building_Bed FindHookupBed(Pawn p1, Pawn p2) {
			Building_Bed result;
			if (p1.ownership.OwnedBed != null) {
				if (p1.ownership.OwnedBed.SleepingSlotsCount > 1) {
					result = p1.ownership.OwnedBed;
					return result;
				}
			}
			if (p2.ownership.OwnedBed != null) {
				if (p2.ownership.OwnedBed.SleepingSlotsCount > 1) {
					result = p2.ownership.OwnedBed;
					return result;
				}
			}
			else {
				foreach (ThingDef current in RestUtility.AllBedDefBestToWorst) {
					if (RestUtility.CanUseBedEver(p1, current)) {
						Building_Bed building_Bed = (Building_Bed)GenClosest.ClosestThingReachable(p1.Position, p1.Map, ThingRequest.ForDef(current), PathEndMode.OnCell, TraverseParms.For(p1, Danger.Deadly, 0, false), 9999f, (Thing x) => true, null, 0, -1, false, RegionType.Set_All, false);
						if (building_Bed != null) {
							if (building_Bed.SleepingSlotsCount > 1) {
								result = building_Bed;
								return result;
							}
						}
					}
				}
			}
			result = null;
			return result;
		}
		public static bool HasNonPolyPartner(Pawn p) {
			bool result;
			foreach (DirectPawnRelation current in p.relations.DirectRelations) {
				if (current.def == PawnRelationDefOf.Lover || current.def == PawnRelationDefOf.Fiance || current.def == PawnRelationDefOf.Spouse) {
					if (!current.otherPawn.story.traits.HasTrait(RRRTraitDefOf.Polyamorous)) {
						result = true;
						return result;
					}
				}
			}
			result = false;
			return result;
		}
		public static bool IsHookupAppealing(Pawn pSubject, Pawn pObject) {
			bool result;
			if (PawnUtility.WillSoonHaveBasicNeed(pSubject)) {
				result = false;
			}
			else {
				float num = 0f;
				num += pSubject.relations.SecondaryRomanceChanceFactor(pObject) / 1.5f;
				num *= Mathf.InverseLerp(-100f, 0f, (float)pSubject.relations.OpinionOf(pObject));
				result = (Rand.Range(0.05f, 1f) < num);
			}
			return result;
		}
		public static bool WillPawnTryHookup(Pawn p1) {
			bool result;
			if (p1.story.traits.HasTrait(RRRTraitDefOf.Asexual)) {
				result = false;
			}
			else {
				Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(p1, false);
				if (pawn != null) {
					float num = (float)p1.relations.OpinionOf(pawn);
					float num2;
					if (p1.story.traits.HasTrait(RRRTraitDefOf.Philanderer)) {
						if (p1.Map == pawn.Map) {
							num2 = Mathf.InverseLerp(70f, 15f, num);
						}
						else {
							num2 = Mathf.InverseLerp(100f, 50f, num);
						}
					}
					else {
						num2 = Mathf.InverseLerp(30f, -80f, num);
					}
					if (p1.story.traits.HasTrait(RRRTraitDefOf.Faithful)) {
						num2 = 0f;
					}
					num2 /= 2f;
					result = (Rand.Range(0f, 1f) < num2);
				}
				else {
					result = true;
				}
			}
			return result;
		}
		public static string GetAdultCulturalAdjective(Pawn p) {
			string result = "Colonial";
			if (p.story.adulthood != null) {
				if (p.story.adulthood.spawnCategories.Contains("Tribal")) {
					result = "Tribal";
				}
				else if (p.story.adulthood.title.Contains("medieval") || p.story.adulthood.baseDesc.IndexOf("Medieval", StringComparison.OrdinalIgnoreCase) >= 0 || p.story.adulthood.baseDesc.IndexOf("Village", StringComparison.OrdinalIgnoreCase) >= 0) {
					result = "Medieval";
				}
				else if (p.story.adulthood.title.Contains("glitterworld") || p.story.adulthood.baseDesc.IndexOf("Glitterworld", StringComparison.OrdinalIgnoreCase) >= 0) {
					if (p.story.adulthood.title != "adventurer") {
						result = "Glitterworld";
					}
				}
				else if (p.story.adulthood.title.Contains("urbworld") || p.story.adulthood.title.Contains("vatgrown") || p.story.adulthood.baseDesc.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0 || p.story.adulthood.baseDesc.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0) {
					result = "Urbworld";
				}
				else if (p.story.adulthood.title.Contains("midworld") || p.story.adulthood.baseDesc.IndexOf("Midworld", StringComparison.OrdinalIgnoreCase) >= 0) {
					result = "Midworld";
				}
				else if (p.story.adulthood.baseDesc.IndexOf("Tribe", StringComparison.OrdinalIgnoreCase) >= 0) {
					result = "Tribal";
				}
				else if (p.story.adulthood.title.Contains("imperial") || p.story.adulthood.baseDesc.IndexOf("Imperial", StringComparison.OrdinalIgnoreCase) >= 0) {
					result = "Imperial";
				}
			}
			return result;
		}
	}

	public class ThoughtWorker_Polyamorous : ThoughtWorker {
		protected override ThoughtState CurrentStateInternal(Pawn p) {
			if (!p.Spawned)
				return ThoughtState.Inactive;
			if (!p.RaceProps.Humanlike)
				return ThoughtState.Inactive;
			if (!p.story.traits.HasTrait(RRRTraitDefOf.Polyamorous))
				return ThoughtState.Inactive;
			if (!LovePartnerRelationUtility.HasAnyLovePartner(p)) {
				return ThoughtState.Inactive;
			}
			List<Pawn> lovers = new List<Pawn>();
			List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
			foreach (DirectPawnRelation rel in directRelations) {
				if (LovePartnerRelationUtility.IsLovePartnerRelation(rel.def) && !rel.otherPawn.Dead) {
					lovers.Add(rel.otherPawn);
				}
			}
			if (lovers.Count == 1 && !lovers[0].story.traits.HasTrait(RRRTraitDefOf.Polyamorous)) {
				return ThoughtState.ActiveAtStage(0);
			}
			return ThoughtState.Inactive;
		}
	}

}
