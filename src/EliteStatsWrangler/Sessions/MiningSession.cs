using EliteAPI.Events;
using System;
using System.Collections.Generic;

namespace EliteStatsWrangler
{
    public class MiningSession : StatSession, IStatSession
    {
        public static string DefaultSessionType = "Mining";
        public MiningSession()//AutoMapper.IMapper objectMapper) : base(objectMapper)
        {
            SessionType = DefaultSessionType;
        }

        internal void AddProspectedAsteroid(string motherlodeMaterial, string contentLocalised, List<ProspectedMaterial> materials)
        {
            this.IncrementStat("Asteroids - Prospected", 1);
            this.IncrementStat($"Asteroids - Prospected - {contentLocalised}", 1);
            if(!string.IsNullOrEmpty(motherlodeMaterial))
                this.IncrementStat($"Asteroids - Prospected - Motherlode - {motherlodeMaterial}", 1);

            materials.ForEach(p => {
                this.IncrementStat($"Asteroids - Material - {p.Name}", 1);
                this.ValueStat($"Asteroids - Material - {p.Name}", p.Proportion);
            });
        }

        internal void AddRefinedMinerals(DateTime timestamp, string typeLocalised)
        {
            this.IncrementStat($"Minerals Refined - {typeLocalised}", 1);
        }

        internal void AddCrackedAsteroid(DateTime timestamp, string typeName)
        {
            this.IncrementStat($"Asteroids - Cracked - {typeName}", 1);
        }

        internal void AddCollectorLaunched(DateTime timestamp)
        {
            this.IncrementStat("Collectors - Launched", 1);
        }

        internal void AddProspectorLaunched(DateTime timestamp)
        {
            this.IncrementStat("Prospectors - Launched", 1);
        }
    }
}