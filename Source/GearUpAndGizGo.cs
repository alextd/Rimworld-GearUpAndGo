using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using UnityEngine;
using RimWorld;
using Verse.Sound;
using Verse.AI;

namespace GearUpAndGo
{
	public class Command_GearUpAndGo : Command
	{
		public Action<IntVec3> action;
		public Action actionEnd;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
			if ((ev.modifiers & EventModifiers.Shift) == EventModifiers.Shift)
				actionEnd();
			else
				Find.Targeter.BeginTargeting(new TargetingParameters() { canTargetLocations = true }, delegate (LocalTargetInfo target)
					{
						this.action(target.Cell);
					});
		}
	}

	[StaticConstructorOnStartup]
	public class TexGearUpAndGo
	{
		public static readonly Texture2D guagIcon = ContentFinder<Texture2D>.Get("CommandGearUpAndGo");
		public static readonly Texture2D guagIconActive = ContentFinder<Texture2D>.Get("CommandGearUpAndGoActive");
	}

	//Backwardcompat
	public class GearUpMapComponent : MapComponent
	{
		public GearUpMapComponent(Map map) : base(map) { }
	}

	public class GearUpPolicyComp : GameComponent
	{
		public string lastPolicy = "";

		public GearUpPolicyComp() : base() { }

		public override void ExposeData()
		{
			Scribe_Values.Look(ref lastPolicy, "lastPolicy", "");
		}
		public void Set()
		{
			if (lastPolicy == "")
			{
				lastPolicy = SetBetterPawnControl.CurrentPolicy();
				SetBetterPawnControl.SetPawnControlPolicy(Settings.Get().betterPawnControlBattlePolicy);
			}
		}
		public void Revert()
		{
			if (lastPolicy == "") return;

			SetBetterPawnControl.SetPawnControlPolicy(lastPolicy);

			lastPolicy = "";
		}
	}

	public class CompGearUpAndGizGo : ThingComp
	{
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (this.parent is Pawn gizmoPawn)
			{
				GearUpPolicyComp component = Current.Game.GetComponent<GearUpPolicyComp>();
				yield return new Command_GearUpAndGo()
				{
					defaultLabel = "TD.GearAndGo".Translate(),
					defaultDesc = "TD.GearAndGoDesc".Translate(),
					icon = component.lastPolicy != "" ? TexGearUpAndGo.guagIconActive : TexGearUpAndGo.guagIcon,
					action = delegate (IntVec3 target)
					{
						Log.Message($"GearUpAndGo to {target}");

						Current.Game.GetComponent<GearUpPolicyComp>().Set();

						foreach (Pawn p in Find.Selector.SelectedObjects
							.Where(o => o is Pawn p && p.IsColonistPlayerControlled).Cast<Pawn>())
						{
							p.jobs.TryTakeOrderedJob(new Job(GearUpAndGoJobDefOf.GearUpAndGo, target), JobTag.DraftedOrder);
						}
					},
					actionEnd = delegate()
					{
						Current.Game.GetComponent<GearUpPolicyComp>().Revert();
					}
				};
			}
		}
	}
}
