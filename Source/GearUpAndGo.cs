using System.Reflection;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using HarmonyLib;

namespace GearUpAndGo
{
	public class Mod : Verse.Mod
	{
		public static Settings settings;
		public Mod(ModContentPack content) : base(content)
		{
			// initialize settings
			settings = GetSettings<Settings>();

#if DEBUG
			Harmony.DEBUG = true;
#endif
			Harmony harmony = new Harmony("uuugggg.rimworld.GearUpAndGo.main");

			harmony.PatchAll();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			settings.DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "TD.GearUpAndGoSettingsName".Translate();
		}
	}

	[DefOf]
	public static class GearUpAndGoJobDefOf
	{
		public static JobDef GearUpAndGo;
	}
}