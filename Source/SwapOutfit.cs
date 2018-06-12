using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;
using PickUpAndHaul;

namespace GearUpAndGo
{
	class SwapOutfit
	{
		public static bool FitsAfterSwap(Apparel newAp, Pawn pawn, Apparel skipAp)
		{
			foreach (Apparel pairAp in pawn.apparel.WornApparel)
			{
				if (pairAp == skipAp) continue;
				Log.Message("Does " + newAp + " fit with " + pairAp + "?");

				if (!ApparelUtility.CanWearTogether(newAp.def, pairAp.def, pawn.RaceProps.body))
				{
					Log.Message("NO");
					return false;
				}
			}
			return true;
		}
		public static Job FindSwapJobs(Pawn pawn)
		{
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			Outfit currentOutfit = pawn.outfits.CurrentOutfit;
			
			//Looping all apparel is a bit redundant because the caller is already in apparel loop, but the order might not work for this
			for (int i = wornApparel.Count - 1; i >= 0; i--)
			{
				Apparel takeOff = wornApparel[i];
				if (!currentOutfit.filter.Allows(takeOff) && pawn.outfits.forcedHandler.AllowedToAutomaticallyDrop(takeOff))
				{
					Log.Message("Finding swaps for " + pawn + ", could take off " + takeOff);

					HashSet<Thing> haulingThings = null;
					Log.Message("Mods: " + ModsConfig.ActiveModsInLoadOrder.ToStringSafeEnumerable());

					try
					{
						((Action)(() =>
						{
							haulingThings = pawn.TryGetComp<CompHauledToInventory>()?.GetHashSet();
						}))();
					}
					catch (TypeLoadException) { }

					foreach (Thing t in pawn.inventory.innerContainer)
					{
						Log.Message("could wear " + t + "?");
						if ((haulingThings == null || !haulingThings.Contains(t)) &&
							t is Apparel swapTo &&
							currentOutfit.filter.Allows(swapTo) &&
							ApparelUtility.HasPartsToWear(pawn, swapTo.def))
						{
							Log.Message("does " + t + " match?");
							if (ApparelUtility.CanWearTogether(takeOff.def, swapTo.def, pawn.RaceProps.body))
								continue;
							Log.Message("does " + t + " fit?");
							if (FitsAfterSwap(swapTo, pawn, takeOff))
							{
								Log.Message("yes totally I'm swapping " + takeOff + " for " + swapTo);
								return new Job(GearUpAndGoJobDefOf.SwapApparelWithInventory, takeOff, swapTo);
							}
						}
					}
					Log.Message("Nothing to swap to: should I return it to inventory?");

					HashSet<Thing> wornThings = pawn.TryGetComp<CompWornFromInventory>()?.GetHashSet();
					if (wornThings?.Contains(takeOff) ?? false)
					{
						Log.Message("yes totally I'm removing " + takeOff + " back into inventory");
						return new Job(GearUpAndGoJobDefOf.SwapApparelWithInventory, takeOff);
					}

				}
			}
			return null;
		}

		public static Job FindEquipJobs(Pawn pawn)
		{
			Log.Message("Finding equip from inv for " + pawn);

			Outfit currentOutfit = pawn.outfits.CurrentOutfit;

			foreach (Apparel apparel in pawn.inventory.innerContainer
				.Where(t => t is Apparel a &&
				ApparelUtility.HasPartsToWear(pawn, a.def) &&
				currentOutfit.filter.Allows(a)))
			{
				Log.Message("Does this work? " + apparel);
				bool conflict = false;
				foreach (Apparel wornApparel in pawn.apparel.WornApparel)
					if (!ApparelUtility.CanWearTogether(wornApparel.def, apparel.def, pawn.RaceProps.body))
					{
						Log.Message("NO. conflicts with " + wornApparel);
						conflict = true;
						continue;
					}
				if (!conflict)
				{
					Log.Message("totally! Wearing " + apparel + "from inventory");
					return new Job(GearUpAndGoJobDefOf.SwapApparelWithInventory, null, apparel);
				}
			}
			return null;
		}

		public static Job FindUpgradeInvJob(Pawn pawn, List<Thing> groundItems)
		{
			Log.Message("Finding if " + pawn + " can upgrade his inventory");

			foreach (Apparel toReplace in pawn.inventory.innerContainer
				.Where(t => t is Apparel a))
			{
				Thing upgradeItem = null;
				float bestScore = JobGiver_OptimizeApparel.ApparelScoreRaw(pawn, toReplace);
				Log.Message("Looking for an upgrade to " + toReplace + "(" + bestScore + ")");

				foreach (Apparel possibleUpgrade in groundItems.Where(i => i is Apparel a
				&& toReplace.def == a.def
				&& a.Map.slotGroupManager.SlotGroupAt(a.Position) != null
				&& !a.IsForbidden(pawn)))
				{
					float thisScore = JobGiver_OptimizeApparel.ApparelScoreRaw(pawn, possibleUpgrade);
					Log.Message("possible upgrade is " + possibleUpgrade + "(" + thisScore + ")");
					if (thisScore > bestScore
						&& pawn.CanReserveAndReach(possibleUpgrade, PathEndMode.OnCell, pawn.NormalMaxDanger(), 1, -1, null, false))
					{
						Log.Message("It's better!");
						upgradeItem = possibleUpgrade;
						bestScore = thisScore;
					}
				}
				if (upgradeItem != null)
					return new Job(GearUpAndGoJobDefOf.UpgradeApparelInInventory, upgradeItem, toReplace);
			}
			return null;
		}
	}

	//Job JobGiver_OptimizeApparel::TryGiveJob(Pawn pawn)
	[HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
	public static class JobGiver_OptimizeApparelPatch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase mb)
		{
			MethodInfo FindSwapJobsInfo = AccessTools.Method(typeof(SwapOutfit), "FindSwapJobs");
			MethodInfo FindEquipJobsInfo = AccessTools.Method(typeof(SwapOutfit), "FindEquipJobs");
			MethodInfo FindUpgradeInvJobInfo = AccessTools.Method(typeof(SwapOutfit), "FindUpgradeInvJob");
			MethodInfo ListGetItemInfo = AccessTools.Property(typeof(List<Apparel>), "Item").GetGetMethod();
			FieldInfo RemoveApparelInfo = AccessTools.Field(typeof(JobDefOf), nameof(JobDefOf.RemoveApparel));


			IList<LocalVariableInfo> locals = mb.GetMethodBody().LocalVariables;
			int thingLocalIndex = locals.First(l => l.LocalType == typeof(Thing)).LocalIndex;
			int thingListLocalIndex = locals.First(l => l.LocalType == typeof(List<Thing>)).LocalIndex;

			bool didFindEquipJobsInfo = false;
			bool foundSetNextOptimizeTick = false;
			foreach (CodeInstruction i in instructions)
			{
				if (i.opcode == OpCodes.Ldsfld && i.operand == RemoveApparelInfo)
				{
					Label jumpToEnd = il.DefineLabel();
					i.labels.Add(jumpToEnd);
					// Job job = FindSwapJobs(pawn, Apparel[i])
					yield return new CodeInstruction(OpCodes.Ldarg_1);//pawn
					yield return new CodeInstruction(OpCodes.Call, FindSwapJobsInfo);//FindSwapJobs(pawn)

					//Save returned job to use it twice
					yield return new CodeInstruction(OpCodes.Stloc_3);
					//job != null
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return new CodeInstruction(OpCodes.Brfalse, jumpToEnd);
					//return job
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return new CodeInstruction(OpCodes.Ret);
				}
				if (i.opcode == OpCodes.Ldarg_0 && !foundSetNextOptimizeTick)
					foundSetNextOptimizeTick = true;
				else if (i.opcode == OpCodes.Ldarg_0)//this.SetNextOptimizeTick(pawn) < - only call using "this". so, check loading this
				{
					Label jumpToEnd = il.DefineLabel();
					i.labels.Add(jumpToEnd);
					// Job job = FindSwapJobs(pawn, Apparel[i])
					yield return new CodeInstruction(OpCodes.Ldarg_1);//pawn
					yield return new CodeInstruction(OpCodes.Ldloc_S, thingListLocalIndex);//thingList
					yield return new CodeInstruction(OpCodes.Call, FindUpgradeInvJobInfo);//FindUpgradeInvJob(pawn, thingList)

					//Save returned job to use it twice
					yield return new CodeInstruction(OpCodes.Stloc_3);
					//job != null
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return new CodeInstruction(OpCodes.Brfalse, jumpToEnd);
					//return job
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return new CodeInstruction(OpCodes.Ret);
				}
				yield return i;
				if (!didFindEquipJobsInfo && i.opcode == OpCodes.Stloc_S &&
					i.operand is LocalBuilder lb && lb.LocalIndex == thingLocalIndex)
				{
					Label jumpToEnd = il.DefineLabel();
					yield return new CodeInstruction(OpCodes.Ldarg_1);//pawn
					yield return new CodeInstruction(OpCodes.Call, FindEquipJobsInfo);//FindSwapJobsInfo(pawn)

					//Save returned job to use it twice
					yield return new CodeInstruction(OpCodes.Stloc_3);
					//job != null
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return new CodeInstruction(OpCodes.Brfalse, jumpToEnd);
					//return job
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return new CodeInstruction(OpCodes.Ret);
					yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label>() { jumpToEnd } };

					didFindEquipJobsInfo = true;
				}
			}
		}
	}
}
