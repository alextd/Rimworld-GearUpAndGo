using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using HarmonyLib;

namespace GearUpAndGo
{
	[StaticConstructorOnStartup]
	static class SetBetterPawnControl
	{
		public static bool working;

		public static bool Active()
		{
			return ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageIdPlayerFacing == "VouLT.BetterPawnControl");
		}

		static SetBetterPawnControl()
		{
			Log.Message("Checking for BCP");
			if (Active())
			{
				working = true;//assume it works till it fails
				Log.Message("Using BCP");
			}
		}
		public static void Stop(Exception e)
		{
			working = false;

			Verse.Log.Error($"Hey, dang, Gear Up and Go failed to use Better Pawn Control:\n\n{e}");
		}

		public static void SetPawnControlPolicy(string policyName)
		{
			if (!working) return;

			try
			{
				SetPawnControlPolicyEx(policyName);
			}
			catch (Exception e)
			{
				Stop(e);
			}
		}

		private static void SetPawnControlPolicyEx(string policyName)
		{
			BetterPawnControl.Policy policy = BetterPawnControl.AssignManager.policies.FirstOrDefault(p => p.label == policyName);

			if (policy != null)
			{
				Log.Message($"using policy: {policy}");
				BetterPawnControl.AssignManager.LoadState(policy);
			}
		}

		public static string CurrentPolicy()
		{
			if (!working) return "";

			try
			{
				return CurrentPolicyEx();
			}
			catch (Exception e)
			{
				Stop(e);
				return "";
			}
		}

		private static string CurrentPolicyEx() =>
			BetterPawnControl.AssignManager.GetActivePolicy()?.label ?? "";

		public static List<string> PolicyList()
		{
			if (!working) return null;

			try
			{
				return PolicyListEx().ToList();
			}
			catch (Exception e)
			{
				Stop(e);
				return null;
			}
		}

		private static IEnumerable<string> PolicyListEx()
		{
			//This oneliner apparently causes problems on load when BCP not active.?
			//BetterPawnControl.AssignManager.policies.Select(p => p.label);

			foreach (BetterPawnControl.Policy p in BetterPawnControl.AssignManager.policies)
				yield return p.label;
		}
	}
}
