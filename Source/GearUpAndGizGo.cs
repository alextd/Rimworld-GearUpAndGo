using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;
using Verse.Sound;
using Verse.AI;
using HarmonyLib;

namespace GearUpAndGo
{
	public class Command_GearUpAndGo : Command
	{
		public Command_GearUpAndGo() : base()
		{
			defaultLabel = "TD.GearAndGo".Translate();
			defaultDesc = "TD.GearAndGoDesc".Translate();
			alsoClickIfOtherInGroupClicked = false;
		}

		public static void Target(string policy = null)
		{
			Find.Targeter.BeginTargeting(new TargetingParameters() { canTargetLocations = true },
				(LocalTargetInfo target) => Go(target, policy));
		}

		public static void Go(LocalTargetInfo target, string policy)
		{
			Log.Message($"GearUpAndGo to {target}, setting {policy}");

			if (!Event.current.alt)
				GearUpPolicyComp.comp.Set(policy);

			foreach (Pawn p in Find.Selector.SelectedObjects
				.Where(o => o is Pawn p && p.IsColonistPlayerControlled).Cast<Pawn>())
			{
				p.jobs.TryTakeOrderedJob(new Job(GearUpAndGoJobDefOf.GearUpAndGo, target), JobTag.DraftedOrder);
			}
		}

		public static void End()
		{
			GearUpPolicyComp.comp.Revert();
		}

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
			if (ev.shift && GearUpPolicyComp.comp.IsOn())
				End();
			else
				Target();
		}

		public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
		{
			get
			{
				var list = SetBetterPawnControl.PolicyList();
				if (list == null) yield break;
				foreach (string policy in list)
					yield return new FloatMenuOption(policy, () => Target(policy));
			}
		}
	}

	[StaticConstructorOnStartup]
	public class TexGearUpAndGo
	{
		public static readonly Texture2D guagIcon = ContentFinder<Texture2D>.Get("CommandGearUpAndGo");
		public static readonly Texture2D guagIconActive = ContentFinder<Texture2D>.Get("CommandGearUpAndGoActive");
	}

	public class GearUpPolicyComp : GameComponent
	{
		public static GearUpPolicyComp comp;

		public string lastPolicy = "";

		public GearUpPolicyComp(Game game) {
			comp = this;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref lastPolicy, "lastPolicy", "");
		}
		public void Set(string policy)
		{
			if (lastPolicy == "")
			{
				lastPolicy = SetBetterPawnControl.CurrentPolicy();
			}
			SetBetterPawnControl.SetPawnControlPolicy(policy ?? Mod.settings.betterPawnControlBattlePolicy);
		}
		public void Revert()
		{
			if (lastPolicy == "") return;

			SetBetterPawnControl.SetPawnControlPolicy(lastPolicy);

			lastPolicy = "";
		}

		public bool IsOn()
		{
			return lastPolicy != "";
		}
	}


	[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
	public static class GearUpAndGizGo
	{
		//	public override IEnumerable<Gizmo> GetGizmos()
		public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
		{
			if (__instance.IsColonistPlayerControlled)
			{
				yield return new Command_GearUpAndGo()
				{
					icon = GearUpPolicyComp.comp.lastPolicy != "" ? TexGearUpAndGo.guagIconActive : TexGearUpAndGo.guagIcon
				};
			}

			foreach (var r in __result)
				yield return r;
		}
	}

	// backcompat dummy so it doesn't log error loading old saves
	public class CompGearUpAndGizGo : ThingComp { }
}
