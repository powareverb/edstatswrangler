using System;

namespace EliteStatsWrangler
{
    public class TravelSession : StatSession, IStatSession
    {
        public static string DefaultSessionType = "Travel";
        private string prevSystem;
        private string prevBody;
        private long? prevMarket;

        public TravelSession()
        {
            SessionType = DefaultSessionType;
        }

        public override void UpdateLocation(CommanderTravelLocation currentLocation)
        {
            // Nothing
            if (currentLocation == null || string.IsNullOrEmpty(currentLocation.SystemName))
                return;

            if (LocationStart == null)
                this.ValueStat("Travel - Start", currentLocation.SystemName);

            base.UpdateLocation(currentLocation);

            if (currentLocation.SystemName.HasValue() && (prevSystem == null || !this.prevSystem.Equals(currentLocation.SystemName)))
            {
                this.IncrementStat("Travel - Jumps", 1);
                this.ValueStat("Travel - Systems", currentLocation.SystemName);
            }

            if (currentLocation.BodyName.HasValue() && (prevBody == null || !this.prevBody.Equals(currentLocation.BodyName)))
            {
                if(currentLocation.BodyType.Equals("Station", StringComparison.OrdinalIgnoreCase))
                {
                    this.IncrementStat("Travel - Stations", 1);
                    this.ValueStat("Travel - Stations", currentLocation.BodyName);
                }
                else
                {
                    this.IncrementStat("Travel - Bodies", 1);
                    this.ValueStat("Travel - Bodies", currentLocation.BodyName);
                }
            }

            if (currentLocation.MarketId.HasValue && (!prevMarket.HasValue || !this.prevMarket.Equals(currentLocation.MarketId)))
            {
                this.IncrementStat("Travel - Markets", 1);
                this.ValueStat("Travel - Markets", currentLocation.MarketId.Value);
            }

            this.prevSystem = currentLocation.SystemName;
            this.prevBody = currentLocation.BodyName;
            this.prevMarket = currentLocation.MarketId;
        }

        protected override void UpdateEndLocation(CommanderTravelLocation currentLocation)
        {
            base.UpdateEndLocation(currentLocation);
            this.ValueStat("Travel - End", currentLocation.SystemName);
        }

        public override IStatSessionSummary SummariseSession(IStatSessionSummary baseline)
        {
            var baseSummary = base.SummariseSession(this, baseline);
            // TODO: Add start and finish locations?
            return baseSummary;
        }
    }
}