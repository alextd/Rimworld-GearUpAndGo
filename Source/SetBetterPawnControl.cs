using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using BetterPawnControl;
using HarmonyLib;

namespace GearUpAndGo
{
	[StaticConstructorOnStartup]
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


		static Type assignManager;
		static Type policyType;
		static FieldInfo policiesInfo;

		static MethodInfo LoadStateInfo;
		public static Delegate LoadStateDelegate;
		static SetBetterPawnControl()
		{
			assignManager = AccessTools.TypeByName("BetterPawnControl.AssignManager");
			Log.Message($"BCP assignManager: {assignManager}");
			policyType = AccessTools.TypeByName("BetterPawnControl.Policy");
			Log.Message($"BCP policyType: {policyType}");
			policiesInfo = AccessTools.Field(assignManager, "policies");
			Log.Message($"BCP policiesInfo: {policiesInfo}");

			LoadStateInfo = AccessTools.Method(assignManager, "LoadState", new Type[] { policyType });
			Log.Message($"BCP LoadStateInfo: {LoadStateInfo}");
			LoadStateDelegate = LoadStateInfo.CreateDelegate(typeof(Action<>).MakeGenericType(policyType));
			Log.Message($"BCP LoadStateDelegate: {LoadStateDelegate}");
		}
		public static void SetPawnControlPolicyEx(string policyName)
		{ 
			List<Policy> assignPolicies = (List<Policy>)policiesInfo.GetValue(default(object));
			Log.Message($"assignPolicies are: {assignPolicies.ToStringSafeEnumerable()}");
			Policy policy = assignPolicies.FirstOrDefault(p => p.label == policyName);
			
			if (policy != null)
			{
				Log.Message($"using policy: {policy}");
				//MainTabWindow_Assign_Policies
				//private static void LoadState(Policy policy)
				LoadStateDelegate.DynamicInvoke(policy);
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
