using EliteAPI.Events;
using System;
using System.Collections.Generic;

namespace EliteStatsWrangler
{
    internal class MissionDetails
    {
        public MissionDetails()
        {
        }

        public string Faction { get; set; }
        public string TargetFaction { get; set; }
        public List<FactionEffect> FactionEffects { get; set; }
        public List<MaterialsReward> MaterialsReward { get; set; }
        public long MissionId { get; internal set; }
        public DateTime Expiry { get; internal set; }
        public DateTime Timestamp { get; internal set; }
        public long PassengerCount { get; internal set; }
        public string CommodityName { get; internal set; }
        public long CommodityCount { get; internal set; }
        public string Influence { get; internal set; }
        public string Reputation { get; internal set; }
    }
}