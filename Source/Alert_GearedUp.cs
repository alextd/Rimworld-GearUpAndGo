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
			this.defaultExplanation = "Gear Up And Go has set Better Pawn Control Policy for a battle.\n\nClick to reset to previous policy" ;
		}

		public override AlertReport GetReport()
		{
			GearUpPolicyComp comp = Current.Game.GetComponent<GearUpPolicyComp>();

			if (comp.lastPolicy == "") return false;

			return true;
		}

		private const float Padding = 6f;
		public override Rect DrawAt(float topY, bool minimized)
		{
			//float height = TexGearUpAndGo.guagIconActive.height;	//The read out really doesn't handle custom heights :/
			float height = Alert.Height;
			Rect rect = new Rect((float)UI.screenWidth - Padding - height, topY, height, height);
			GUI.color = Color.white;
			GUI.DrawTexture(rect, TexGearUpAndGo.guagIconActive);
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			if (Widgets.ButtonInvisible(rect, false))
			{
				Current.Game.GetComponent<GearUpPolicyComp>().Revert();
			}
			return rect;
		}
	}
}