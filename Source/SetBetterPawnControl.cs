using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using BetterPawnControl;
using Harmony;

namespace GearUpAndGo
{
	static class SetBetterPawnControl
	{
		public static bool Active()
		{
			return ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Better Pawn Control");
		}

		static bool warned = false;
		//Okay, I want it really bad, so let's do this.
		public static void SetPawnControlPolicy(string policyName)
		{
			if (!Active()) return;

			try
			{
				SetPawnControlPolicyEx(policyName);
			}
			catch(Exception e)
			{
				if (!warned)
				{
					string desc = "Gear Up And Go Failed to set Better Pawn Control Policy, I have no idea why, probably needs a re-compile, so check for an update or tell me (sorry this is really confusing.)";
					Find.LetterStack.ReceiveLetter("Gear+Go failed to set Pawn Control", desc, LetterDefOf.NegativeEvent, e.ToStringSafe());
					Verse.Log.Error($"{desc}\n\n{e.ToStringSafe()}");
					warned = true;
				}
			}
		}

		static Type assignManager = AccessTools.TypeByName("AssignManager");
		static FieldInfo linksInfo = AccessTools.Field(assignManager, "links");
		static FieldInfo policiesInfo = AccessTools.Field(assignManager, "policies");
		static MethodInfo LoadStateInfo = AccessTools.Method(AccessTools.TypeByName("AssignManager"), "LoadState");
		public static void SetPawnControlPolicyEx(string policyName)
		{ 
			List<AssignLink> links = (List<AssignLink>)linksInfo.GetValue(default(object));
			Log.Message($"links are: {links.ToStringSafeEnumerable()}");

			List<Policy> assignPolicies = (List<Policy>)policiesInfo.GetValue(default(object));
			Log.Message($"assignPolicies are: {assignPolicies.ToStringSafeEnumerable()}");

			List<Pawn> pawns = Find.CurrentMap.mapPawns.FreeColonists.ToList();
			Log.Message($"pawns are: {pawns.ToStringSafeEnumerable()}");

			Policy policy = assignPolicies.FirstOrDefault(p => p.label == policyName);
			
			if (policy != null)
			{
				Log.Message($"using policy: {policy}");
				//MainTabWindow_Assign_Policies
				//private static void LoadState(List<AssignLink> links, List< Pawn > pawns, Policy policy)
				LoadStateInfo.Invoke(default(object), new object[] {links, pawns, policy});
			}
		}

		public static string CurrentPolicy()
		{
			if (!Active()) return "";

			try
			{
				return CurrentPolicyEx();
			}
			catch (Exception)
			{
				return "";
			}
		}

		static MethodInfo GetActivePolicyInfo = AccessTools.Method(AccessTools.TypeByName("AssignManager"), "GetActivePolicy", new Type[] { });
		public static string CurrentPolicyEx()
		{ 
			Log.Message($"Resetting policies");

			return (GetActivePolicyInfo.Invoke(null, new object[] { }) as Policy)?.label ?? "";
		}

		public static List<string> PolicyList()
		{
			if (!Active()) return null;

			try
			{
				return PolicyListEx();
			}
			catch (Exception)
			{
				return null;
			}
		}
		
		public static List<string> PolicyListEx()
		{
			//Hitting can't load type errors if I make this an easier yield return, so messy lists it is
			List<string> policyNames = new List<string>();
			List<Policy> assignPolicies = (List<Policy>)policiesInfo.GetValue(default(object));
			Log.Message($"assignPolicies are: {assignPolicies.ToStringSafeEnumerable()}");
			foreach (Policy p in assignPolicies)
			{
				policyNames.Add(p.label);
			}

			return policyNames;
		}
	}
}
