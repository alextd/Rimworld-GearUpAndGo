using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GearUpAndGo
{
    public class JobDriver_SwapApparelWithInventory : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (this.TargetA != null)
            {
                yield return new Toil
                {
                    initAction = delegate
                    {
                        this.pawn.pather.StopDead();
                    },
                    defaultCompleteMode = ToilCompleteMode.Delay,
                    defaultDuration = 100
                };
                yield return new Toil
                {
                    initAction = () =>
                    {
                        Pawn actor = this.pawn;
                        Thing thing = actor.CurJob.GetTarget(TargetIndex.A).Thing;

                        actor.inventory.innerContainer.TryAddOrTransfer(thing);

                        pawn.TryGetComp<CompWornFromInventory>()?.GetHashSet().Add(thing);

                        if (actor.CurJob.GetTarget(TargetIndex.B) == null)
                            this.EndJobWith(JobCondition.Succeeded);
                    }
                };
            }
            if (this.TargetB == null)
                yield break;
            yield return new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 100
            };
            yield return new Toil
            {
                initAction = () =>
                {
                    Pawn actor = this.pawn;
                    Apparel thing = actor.CurJob.GetTarget(TargetIndex.B).Thing as Apparel;

                    //actor.apparel.innerContainer.TryTransfer(thing);
                    // apparel doesn't allow access to thingholder so must manually remove
                    actor.inventory.innerContainer.Remove(thing);
                    actor.apparel.Wear(thing);

                    pawn.TryGetComp<CompWornFromInventory>()?.RegisterWornItem(thing);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

    }

    public class JobDriver_UpgradeApparelInInventory : JobDriver_Wear
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toils = base.MakeNewToils().ToList();
            toils.RemoveLast();
            Toil replaceToil = new Toil
            {
                initAction = () =>
                {
                    Apparel apparel = (Apparel)this.job.targetA.Thing;
                    Apparel dropThis = (Apparel)this.job.targetB.Thing;

                    Log.Message(this.pawn+" dropping " + dropThis);
                    this.pawn.inventory.innerContainer.TryDrop(dropThis, ThingPlaceMode.Near, 1, out Thing dummy);
                    Log.Message(this.pawn + " equippin " + apparel);
                    apparel.DeSpawn();
                    this.pawn.inventory.innerContainer.TryAdd(apparel);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            toils.Add(replaceToil);
            return toils;
        }
    }
    
    [DefOf]
    public static class GearUpAndGoJobDefOf
    {
        public static JobDef SwapApparelWithInventory;
        public static JobDef UpgradeApparelInInventory;
    }
}