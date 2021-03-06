﻿using System.Reflection;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace GearUpAndGo
{
	public class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content)
		{
			// initialize settings
			GetSettings<Settings>();
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
		public static JobDef GearUpAndGo;
	}
}