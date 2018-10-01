using System.Reflection;
using Verse;
using Verse.AI;
using RimWorld;
using Harmony;
using UnityEngine;

namespace GearUpAndGo
{
	public class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content)
		{
			// initialize settings
			GetSettings<Settings>();
#if DEBUG
			HarmonyInstance.DEBUG = true;
#endif
			HarmonyInstance harmony = HarmonyInstance.Create("uuugggg.rimworld.GearUpAndGo.main");

			//Turn off DefOf warning since harmony patches trigger it.
			harmony.Patch(AccessTools.Method(typeof(DefOfHelper), "EnsureInitializedInCtor"),
				new HarmonyMethod(typeof(Mod), "EnsureInitializedInCtorPrefix"), null);
			
			harmony.PatchAll();
		}

		public static bool EnsureInitializedInCtorPrefix()
		{
			//No need to display this warning.
			return false;
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			GetSettings<Settings>().DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "TD.GearUpAndGoSettingsName".Translate();
		}
	}

	[DefOf]
	public static class GearUpAndGoJobDefOf
	{
		/* UNTIL 1.0 UPDATES
			public static JobDef SwapApparelWithInventory;
			public static JobDef UpgradeApparelInInventory;
		*/
		public static JobDef GearUpAndGo;
	}
}