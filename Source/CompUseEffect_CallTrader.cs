using RimWorld;
using HugsLib;
using System;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace TraderWhereAreYou
{
    public class CompUseEffect_CallTrader : CompUseEffect
    {
        private bool EnoughSilver(Map map)
        {
            int total = 0;

            return EnoughSilver(map, out total);
        }

        private bool EnoughSilver(Map map, out int totalSilver)
        {
            List<Thing> silverStacks = TradeUtility.AllLaunchableThingsForTrade(map)
                .Where(x => x.def == ThingDefOf.Silver).ToList();

            var total = 0;
            var cost = ModShared.Settings.GetHandle<int>("cost").Value;

            foreach (Thing s in silverStacks)
            {
                total += s.stackCount;
                if (total >= cost)
                {
                    totalSilver = cost;
                    return true;
                }
            }

            totalSilver = total;
            return false;
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            Map map = p.Map;
            int total;

            if (map.passingShipManager.passingShips.Count >= 5)
            {
                failReason = "MaxShipsInOrbit".Translate();
                return false;
            }
            else if (!EnoughSilver(map, out total))
            {
                failReason = "InsufficientSilverBalance".Translate(300 - total);
                return false;
            }

            failReason = null;
            return true;
        }
        
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            Map map = usedBy.Map;
            if (map.passingShipManager.passingShips.Count >= 5)
            {
                return;
            }
            else if (!EnoughSilver(map))
            {
                return;
            }

            List<Thing> silverStacks = TradeUtility.AllLaunchableThingsForTrade(map)
                .Where(x => x.def == ThingDefOf.Silver).ToList();

            var cost = ModShared.Settings.GetHandle<int>("cost").Value;
            foreach (Thing s in silverStacks)
            {
                Log.Message("Counter: " + cost);
                Log.Message("Stack Count: " + s.stackCount);

                if (s.stackCount > cost)
                {
                    Log.Message("Stack can pay full amount.");
                    Log.Message("Stack Count Before Subtraction: " + s.stackCount);
                    s.stackCount -= cost;
                    Log.Message("Stack Count After Subtraction: " + s.stackCount);

                    break;
                }

                cost -= s.stackCount;
                s.Destroy();
            }

            map.resourceCounter.UpdateResourceCounts();

            if (DefDatabase<TraderKindDef>.AllDefs
                .Where(x => x.orbital)
                .TryRandomElementByWeight(traderDef => traderDef.commonality, out TraderKindDef def))
            {
                TradeShip tradeShip = new TradeShip(def);
                if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn))
                {
                    Find.LetterStack.ReceiveLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(
                        tradeShip.name,
                        tradeShip.def.label,
                        "TraderArrivalNoFaction".Translate()
                    ), LetterDefOf.PositiveEvent, null);
                }
                map.passingShipManager.AddShip(tradeShip);
                tradeShip.GenerateThings();
            }
            else throw new InvalidOperationException();

        }
    }
}