using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace GearUpAndGo
{
	public class Alert_GearedUp : Alert
	{
		public Alert_GearedUp()
		{
			this.defaultExplanation = "TD.GearUpPolicySetAlert".Translate() ;
		}

		public override AlertReport GetReport()
		{
			return GearUpPolicyComp.comp.IsOn();
		}

		private const float Padding = 6f;
		public override Rect DrawAt(float topY, bool minimized)
		{
			//float height = TexGearUpAndGo.guagIconActive.height;	//The read out really doesn't handle custom heights :/
			float height = Height;
			Rect rect = new Rect((float)UI.screenWidth - Padding - height, topY, height, height);
			GUI.color = Color.white;
			GUI.DrawTexture(rect, TexGearUpAndGo.guagIconActive);
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if (Widgets.ButtonInvisible(rect, false))
			{
				GearUpPolicyComp.comp.Revert();
			}
			return rect;
		}
	}
}