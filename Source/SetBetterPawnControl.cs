using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
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

		//Okay, I want it really bad, so let's do this.
		public static void SetPawnControlPolicy(string policyName)
		{
			if (!Active()) return;

			Type assignMananer = AccessTools.TypeByName("AssignManager");

			FieldInfo linksInfo = AccessTools.Field(assignMananer, "links");
			List<AssignLink> links = (List<AssignLink>)linksInfo.GetValue(default(object));
			Log.Message("links are: " + links.ToStringSafeEnumerable());

			FieldInfo policiesInfo = AccessTools.Field(assignMananer, "policies");
			List<Policy> assignPolicies = (List<Policy>)policiesInfo.GetValue(default(object));
			Log.Message("assignPolicies are: " + assignPolicies.ToStringSafeEnumerable());

			List<Pawn> pawns = Find.VisibleMap.mapPawns.FreeColonists.ToList();
			Log.Message("pawns are: " + pawns.ToStringSafeEnumerable());

			Policy policy = assignPolicies.FirstOrDefault(p => p.label == policyName);
			
			if (policy != null)
			{
				Log.Message("using policy: " + policy);
				//MainTabWindow_Assign_Policies
				//private static void LoadState(List<AssignLink> links, List< Pawn > pawns, Policy policy)
				MethodInfo LoadStateInfo = AccessTools.Method(AccessTools.TypeByName("MainTabWindow_Assign_Policies"), "LoadState");
				LoadStateInfo.Invoke(default(object), new object[] {links, pawns, policy});
			}
		}

		public static string CurrentPolicy()
		{
			if (!Active()) return "";

			Type assignMananer = AccessTools.TypeByName("AssignManager");

			Log.Message("Resetting policyies");

			MethodInfo GetActivePolicyInfo = AccessTools.Method(AccessTools.TypeByName("AssignManager"), "GetActivePolicy", new Type[] { });
			return (GetActivePolicyInfo.Invoke(null, new object[] { }) as Policy)?.label ?? "";
		}
	}
}
