using System.Collections.Generic;
using Verse;

namespace GearUpAndGo
{
	public class CompWornFromInventory : ThingComp
	{
		private HashSet<Thing> WornFromInventory = new HashSet<Thing>();
		
		public HashSet<Thing> GetHashSet()
		{
			return WornFromInventory;
		}

		public void RegisterWornItem(Thing thing)
		{
			this.WornFromInventory.Add(thing);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look<Thing>(ref this.WornFromInventory, "ThingsWornFromInventory", LookMode.Reference);
		}
	}
}
