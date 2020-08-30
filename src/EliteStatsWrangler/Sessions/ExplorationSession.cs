using EliteAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteStatsWrangler.Sessions
{
    public class ExplorationSession : StatSession, IStatSession
    {
        public static string DefaultSessionType = "Exploration";
        public ExplorationSession()
        {
            SessionType = DefaultSessionType;
        }

        internal void ScoopedFuel(float scooped)
        {
            this.IncrementStat($"Exploration - Fuel Scooped", scooped);
        }

        internal void SoldExplorationData(SellExplorationDataInfo e, CommanderTravelLocation currentLocation)
        {
            // TODO
            this.IncrementStat($"Exploration - Data", e.TotalEarnings);
            if (currentLocation.BodyType.Equals("Station", StringComparison.OrdinalIgnoreCase))
                this.IncrementStat($"Exploration - Carto Data - {currentLocation.SystemName} - {currentLocation.BodyName}", e.BaseValue);
            else
                this.IncrementStat($"Exploration - Carto Data - {currentLocation.SystemName} - MarketId:{currentLocation.MarketId}", e.BaseValue);
        }
    }
}
