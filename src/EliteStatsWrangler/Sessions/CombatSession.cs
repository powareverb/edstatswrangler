using EliteAPI.Events.Travel;
using System;
using System.Collections.Generic;

namespace EliteStatsWrangler
{
    public class CombatSession : StatSession, IStatSession
    {
        public static string DefaultSessionType = "Combat";
        public CombatSession()
        {
            SessionType = DefaultSessionType;
        }

        internal void CockpitBreached()
        {
            this.IncrementStat($"Combat - Cockpit Breached", 1);
        }

        internal void RedeemedBountyVoucher(long amount, List<FSDJumpFaction> factions, CommanderTravelLocation currentLocation)
        {
            //  Need to figure how bounty vouchers effect anything
            this.IncrementStat($"Combat - Bounty Vouchers", amount);
            if(currentLocation.BodyType.Equals("Station", StringComparison.OrdinalIgnoreCase))
                this.IncrementStat($"Combat - Bounty Vouchers - {currentLocation.SystemName} - {currentLocation.BodyName}", amount);
            else
                this.IncrementStat($"Combat - Bounty Vouchers - {currentLocation.SystemName} - MarketId:{currentLocation.MarketId}", amount);
        }
    }
}