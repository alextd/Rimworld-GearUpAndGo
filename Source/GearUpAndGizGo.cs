using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using UnityEngine;
using RimWorld;
using Verse.Sound;

namespace GearUpAndGo
{
	public class Command_GearUpAndGo : Command
	{
		public Action<IntVec3> action;
		public Action actionEnd;

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.TickTiny.PlayOneShotOnCamera(null);
			if (ev.button == 0)
				Find.Targeter.BeginTargeting(new TargetingParameters() { canTargetLocations = true }, delegate (LocalTargetInfo target)
					{
						this.action(target.Cell);
					});
			else
				actionEnd();
		}
	}

	[StaticConstructorOnStartup]
	public class TexGearUpAndGo
	{
		public static readonly Texture2D guagIcon = ContentFinder<Texture2D>.Get("CommandGearUpAndGo");
		public static readonly Texture2D guagIconActive = ContentFinder<Texture2D>.Get("CommandGearUpAndGoActive");
	}

	public class GearUpMapComponent : MapComponent
	{
		public bool active = false;
		public string lastPolicy = "";

		public GearUpMapComponent(Map map) : base(map) { }

		public override void ExposeData()
		{
			Scribe_Values.Look(ref active, "active", false);
			Scribe_Values.Look(ref lastPolicy, "lastPolicy", "");
		}
	}

	public class CompGearUpAndGizGo : ThingComp
	{
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (this.parent is Pawn gizmoPawn)
			{
				GearUpMapComponent component = gizmoPawn.Map?.GetComponent<GearUpMapComponent>();
				yield return new Command_GearUpAndGo()
				{
					defaultLabel = "TD.GearAndGo".Translate(),
					defaultDesc = "TD.GearAndGoDesc".Translate(),
					icon = component.active ? TexGearUpAndGo.guagIconActive : TexGearUpAndGo.guagIcon,
					action = delegate (IntVec3 target)
					{
						Log.Message("GearUpAndGo to " + target);

						GearUpMapComponent comp = gizmoPawn.Map?.GetComponent<GearUpMapComponent>();
						comp.active = true;

						if (comp.lastPolicy == "")
						{
							comp.lastPolicy = SetBetterPawnControl.CurrentPolicy();
							SetBetterPawnControl.SetPawnControlPolicy(Settings.Get().betterPawnControlBattlePolicy);
						}

						foreach (Pawn p in Find.Selector.SelectedObjects
							.Where(o => o is Pawn p && p.IsColonistPlayerControlled).Cast<Pawn>())
						{
							p.jobs.StartJob(new Verse.AI.Job(GearUpAndGoJobDefOf.GearUpAndGo, target), Verse.AI.JobCondition.InterruptForced);
						}
					},
					actionEnd = delegate()
					{
						GearUpMapComponent comp = gizmoPawn.Map?.GetComponent<GearUpMapComponent>();
						if (!comp.active) return;
						comp.active = false;

						Log.Message("GearUpAndGo done");

						SetBetterPawnControl.SetPawnControlPolicy(comp.lastPolicy);
						comp.lastPolicy = "";

						foreach (Pawn p in Find.Selector.SelectedObjects
							.Where(o => o is Pawn p && p.IsColonistPlayerControlled).Cast<Pawn>())
						{
							p.jobs.ClearQueuedJobs();
							p.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
							p.drafter.Drafted = false;
						}
					}
				};
			}
		}
	}
}
