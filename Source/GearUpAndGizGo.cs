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

		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.TickTiny.PlayOneShotOnCamera(null);
			Find.Targeter.BeginTargeting(new TargetingParameters() { canTargetLocations = true }, delegate (LocalTargetInfo target)
				{
					this.action(target.Cell);
				});
		}
	}

	[StaticConstructorOnStartup]
	public class TexGearUpAndGo
	{
		public static readonly Texture2D guagIcon = ContentFinder<Texture2D>.Get("CommandGearUpAndGo", true);
	}
	public class CompGearUpAndGizGo : ThingComp
	{
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (this.parent is Pawn gizmoPawn)
			{
				yield return new Command_GearUpAndGo()
				{
					defaultLabel = "Gear & Go",
					defaultDesc = "Draft, get gear for current outfit, then go to the targeted location",
					icon = TexGearUpAndGo.guagIcon,
					action = delegate (IntVec3 target)
					{
						Log.Message("GearUpAndGo to " + target);
						foreach (Pawn p in Find.Selector.SelectedObjects
							.Where(o => o is Pawn p && p.IsColonistPlayerControlled).Cast<Pawn>())
						{
							p.jobs.StartJob(new Verse.AI.Job(GearUpAndGoJobDefOf.GearUpAndGo, target), Verse.AI.JobCondition.InterruptForced);
						}
					}
				};
			}
		}
	}
}
