using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace GearUpAndGo
{
	class Settings : ModSettings
	{
		public string betterPawnControlBattlePolicy = "";

		public static Settings Get()
		{
			return LoadedModManager.GetMod<GearUpAndGo.Mod>().GetSettings<Settings>();
		}

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			options.Begin(wrect);

			options.Label("If you're using Better Pawn Control, set this policy when you click the button:");
			betterPawnControlBattlePolicy = options.TextEntry(betterPawnControlBattlePolicy, 1);
			options.Label("The last policy is remembered, so right-clicking the button will reset and undraft");
			options.Gap();

			options.End();
		}
		
		public override void ExposeData()
		{
			Scribe_Values.Look(ref betterPawnControlBattlePolicy, "betterPawnControlBattlePolicy", "");
		}
	}
}