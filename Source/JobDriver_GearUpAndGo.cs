using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace GearUpAndGo
{
	public class JobDriver_GearUpAndGo : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		//Combat Extended Support
		public static Type CEloadoutGiverType = AccessTools.TypeByName("JobGiver_UpdateLoadout");
		public static MethodInfo CEloadoutGetter;
		public static MethodInfo TryIssueJobPackageInfo = AccessTools.Method(typeof(ThinkNode), nameof(ThinkNode.TryIssueJobPackage));

		//Weapon of Choice support
		public static Type WOCGiverType = AccessTools.TypeByName("JobGiver_OptimizeEquipment");
		public static MethodInfo WOCGetter;
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn pawn = toil.actor;
				if (pawn.thinker == null) return;
				
				//Find apparel
				JobGiver_OptimizeApparel optimizer = pawn.thinker.TryGetMainTreeThinkNode<JobGiver_OptimizeApparel>();
				if (optimizer == null) return;

				pawn.mindState?.Notify_OutfitChanged();// Lie so that it re-equips things
				ThinkResult result = optimizer.TryIssueJobPackage(pawn, new JobIssueParams()); //TryGiveJob is protected :(

				//Find loadout, Combat Extended
				if (result == ThinkResult.NoJob)
				{
					if (CEloadoutGiverType != null)
					{
						if (CEloadoutGetter == null)
							CEloadoutGetter = AccessTools.Method(typeof(Pawn_Thinker), nameof(Pawn_Thinker.TryGetMainTreeThinkNode)).MakeGenericMethod(new Type[] { CEloadoutGiverType });
						if (CEloadoutGetter != null)
						{
							object CELoadoutGiver = CEloadoutGetter.Invoke(pawn.thinker, new object[] { });
							if (CELoadoutGiver != null) 
								result = (ThinkResult)TryIssueJobPackageInfo.Invoke(CELoadoutGiver, new object[] { pawn, new JobIssueParams() });
						}
					}
				}
				//Find weapons, Weapons of Choice
				if (result == ThinkResult.NoJob)
				{
					if (WOCGiverType != null)
					{
						if (WOCGetter == null)
							WOCGetter = AccessTools.Method(typeof(Pawn_Thinker), nameof(Pawn_Thinker.TryGetMainTreeThinkNode)).MakeGenericMethod(new Type[] { WOCGiverType });
						if (WOCGetter != null)
						{
							object WOCLoadoutGiver = WOCGetter.Invoke(pawn.thinker, new object[] { });
							if (WOCLoadoutGiver != null)
								result = (ThinkResult)TryIssueJobPackageInfo.Invoke(WOCLoadoutGiver, new object[] { pawn, new JobIssueParams() });
						}
					}
				}
				//Okay, nothing to do, go to target
				if (result == ThinkResult.NoJob)
				{
					IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(TargetA.Cell, pawn);
					Job job = new Job(JobDefOf.Goto, intVec);
					if (pawn.Map.exitMapGrid.IsExitCell(UI.MouseCell()))
					{
						job.exitMapOnArrival = true; // I guess
					}

					if (!pawn.Drafted)
					{
						//Drafting clears the job queue. We want to keep the queue.
						//It'll also return jobs to the pool, and clear each job too.
						//So extract each job and clear the queue manually, then re-queue them all.

						List<QueuedJob> queue = new List<QueuedJob>();
						while (pawn.jobs.jobQueue.Count > 0)
							queue.Add(pawn.jobs.jobQueue.Dequeue());

						pawn.drafter.Drafted = true;

						pawn.jobs.StartJob(job, JobCondition.Succeeded);

						foreach (QueuedJob qj in queue)
							pawn.jobs.jobQueue.EnqueueLast(qj.job, qj.tag);
					}
					else
						pawn.jobs.StartJob(job, JobCondition.Succeeded);

					FleckMaker.Static(intVec, pawn.Map, FleckDefOf.FeedbackGoto);
				}
				//Queue up the Gear job, then do another Gear+Go job
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
