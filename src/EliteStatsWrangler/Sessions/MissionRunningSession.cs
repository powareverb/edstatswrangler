using System;
using System.Collections.Generic;
using System.Linq;

namespace EliteStatsWrangler
{
    public class MissionRunningSession : StatSession, IStatSession
    {
        public static string DefaultSessionType = "Missions";
        List<MissionDetails> missionList = new List<MissionDetails>();

        public MissionRunningSession()
        {
            SessionType = DefaultSessionType;
        }

        internal void CompletedMission(MissionDetails deets)
        {
            var origMission = missionList.Where(p => p.MissionId == deets.MissionId).FirstOrDefault();

            if (origMission != null)
            {
                this.IncrementStat("Missions - Completed", 1);
                this.IncrementStat($"Missions - Completed - {deets.Faction}", 1);

                if (deets.FactionEffects.Any())
                {
                    if(deets.FactionEffects.Any(p => p.Influence.Any()))
                    {
                        foreach(var factEffect in deets.FactionEffects)
                        {
                            if(!string.IsNullOrEmpty(factEffect.Reputation))
                            {
                                var repNum = factEffect.Reputation.Length;
                                if (!factEffect.ReputationTrend.StartsWith("Up"))
                                    repNum = -repNum;

                                this.IncrementStat($"Missions - FactionRep - {factEffect.Faction}", repNum);
                            }
                            foreach(var infEffect in factEffect.Influence)
                            {
                                // Based on infEffect
                                {
                                    var infNum = infEffect.InfluenceInfluence.Length;
                                    if (!infEffect.Trend.StartsWith("Up"))
                                        infNum = -infNum;

                                    this.IncrementStat($"Missions - FactionInfluence - {factEffect.Faction} - SystemAddress:{infEffect.SystemAddress}", infNum);
                                }
                                // Based on initial mission inf
                                {
                                    var infNum = origMission.Influence.Length;
                                    if (!infEffect.Trend.StartsWith("Up"))
                                        infNum = -infNum;

                                    this.IncrementStat($"Missions - FactionInfluence2 - {factEffect.Faction} - SystemAddress:{infEffect.SystemAddress}", infNum);
                                }
                            }
                        }
                    }
                }
            } 
            else
            {
                // ?
            }
        }

        internal void AcceptedMission(MissionDetails deets)
        {
            this.IncrementStat("Missions - Accepted", 1);
            this.IncrementStat($"Missions - Accepted - {deets.Faction}", 1);
            this.ValueStat("Missions - Factions", deets.Faction);

            missionList.Add(deets);
        }

        internal void FailedMission(MissionDetails deets)
        {
            var origMission = missionList.Where(p => p.MissionId == deets.MissionId).FirstOrDefault();

            if (origMission != null)
            {
                this.IncrementStat("Missions - Failed", 1);
            }
            else
            {
                // ?
            }
        }

        internal void AbandonedMission(MissionDetails deets)
        {
            var origMission = missionList.Where(p => p.MissionId == deets.MissionId).FirstOrDefault();

            if (origMission != null)
            {
                this.IncrementStat("Missions - Abandoned", 1);
            }
            else
            {
                // ?
            }
        }


        public override void StartSession(DateTime timestamp, string reason)
        {
            // TODO: Make sure we've got missions loaded
            base.StartSession(timestamp, reason);
        }

        public override void EndSession(DateTime timestamp, string reason)
        {
            // TODO: Make sure we've got missions saved
            base.EndSession(timestamp, reason);
        }
    }
}