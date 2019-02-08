using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace GearUpAndGo
{
	public class JobDriver_GearUpAndGo : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn pawn = toil.actor;
				if (pawn.thinker == null) return;
				
				JobGiver_OptimizeApparel optimizer = pawn.thinker.TryGetMainTreeThinkNode<JobGiver_OptimizeApparel>();
				if (optimizer == null) return;

				pawn.mindState?.Notify_OutfitChanged();// Lie so that it re-equips things
				ThinkResult result = optimizer.TryIssueJobPackage(pawn, new JobIssueParams()); //TryGiveJob is protected :(
				if (result == ThinkResult.NoJob)
				{
					Log.Message($"{pawn} JobDriver_GearUpAndGo result NoJob");
					IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(TargetA.Cell, pawn);
					Job job = new Job(JobDefOf.Goto, intVec);
					if (pawn.Map.exitMapGrid.IsExitCell(UI.MouseCell()))
					{
						job.exitMapOnArrival = true; // I guess
					}

					//preserve the queue
					List<QueuedJob> queue = pawn.Drafted ? new List<QueuedJob>() : new List<QueuedJob>(pawn.jobs.jobQueue);

					pawn.drafter.Drafted = true;
					pawn.jobs.StartJob(job, JobCondition.Succeeded);
					foreach (QueuedJob qj in queue)
					{
						pawn.jobs.jobQueue.EnqueueLast(qj.job, qj.tag);
					}

					MoteMaker.MakeStaticMote(intVec, pawn.Map, ThingDefOf.Mote_FeedbackGoto, 1f);
				}
				else
				{
					Job optJob = result.Job;
					Log.Message($"{pawn} JobDriver_GearUpAndGo job {optJob}");
					if (optJob.def == JobDefOf.Wear)
						pawn.Reserve(optJob.targetA, optJob);
					pawn.jobs.jobQueue.EnqueueFirst(new Job(GearUpAndGoJobDefOf.GearUpAndGo, TargetA));
					pawn.jobs.jobQueue.EnqueueFirst(optJob);
				}
			};
			yield return toil;
		}
	}
}
