using System.Collections.Generic;
using System;
using AutoMapper;

namespace EliteStatsWrangler
{
    public class StatSession : IStatSession
    {
        public DateTime SessionStarted { get; set; }
        public DateTime? SessionEnded { get; set; }
        public string ReasonStarted { get; internal set; }
        public string ReasonEnded { get; internal set; }
        public string TimeStamp { get { return String.Format("{0:yyyyMMdd-HHmmss}", SessionStarted); } }

        public TimeSpan SessionTimeConsumed {  get { return (SessionEnded.HasValue ? SessionEnded.Value : DateTime.Now) - SessionStarted; } }

        public string SessionCommanderName { get; set; }
        public string SessionType { get; internal set; }
        public CommanderTravelLocation LocationCurrent { get ; protected set; }
        public CommanderTravelLocation LocationStart { get; protected set; }
        public CommanderTravelLocation LocationEnd { get; protected set; }

        public string SessionShipName { get; set; }
        public string SessionShipIdent { get; set; }

        public Dictionary<string, long> _summaryStats = new Dictionary<string, long>();

        public Dictionary<string, List<double>> _doubleValueStats = new Dictionary<string, List<double>>();
        public Dictionary<string, List<string>> _stringValueStats = new Dictionary<string, List<string>>();
        public Dictionary<string, long> SummaryStats { get { return _summaryStats; } }
        public Dictionary<string, List<double>> DoubleValueStats { get { return _doubleValueStats; } }

        // FIXME: Janker
        //protected IMapper ObjectMapper { get; set; }
        //public StatSession(IMapper objectMapper)
        //{
        //    ObjectMapper = objectMapper;
        //}

        protected void IncrementStat(string statName, float statInc)
        {
            // TODO: Implement, for now just bodge
            if (!this._summaryStats.ContainsKey(statName))
                this._summaryStats.Add(statName, (long)statInc);
            else
                this._summaryStats[statName] = this._summaryStats[statName] + (long)statInc;
        }
        protected void IncrementStat(string statName, long statInc)
        {
            if (!this._summaryStats.ContainsKey(statName))
                this._summaryStats.Add(statName, statInc);
            else
                this._summaryStats[statName] = this._summaryStats[statName] + statInc;
        }

        protected void ValueStat(string statName, string value)
        {
            if (!this._stringValueStats.ContainsKey(statName))
                this._stringValueStats.Add(statName, new List<string>());

            this._stringValueStats[statName].Add(value);
        }

        protected void ValueStat(string statName, double value)
        {
            if(!this._doubleValueStats.ContainsKey(statName))
                this._doubleValueStats.Add(statName, new List<double>());

            this._doubleValueStats[statName].Add(value);
        }

        /// <summary>
        /// Don't overwrite if we've already got details
        /// </summary>
        /// <param name="activeSessions"></param>
        public void SetSessionDefaults(ActiveSessions activeSessions)
        {
            if (string.IsNullOrEmpty(this.SessionCommanderName))
                this.SessionCommanderName = activeSessions.CommanderName;
            UpdateLocation(activeSessions.CurrentLocation);

            if (string.IsNullOrEmpty(this.SessionShipName))
                this.SessionShipName = activeSessions.CurrentShip;
            if (string.IsNullOrEmpty(this.SessionShipIdent))
                this.SessionShipIdent = activeSessions.CurrentShipIdent;
        }

        public virtual void StartSession(DateTime timestamp, string reason)
        {
            this.SessionStarted = timestamp;
            this.ReasonStarted = reason;
        }
        public virtual void EndSession(DateTime timestamp, string reason)
        {
            UpdateEndLocation(LocationCurrent);

            this.SessionEnded = timestamp;
            this.ReasonEnded = reason;
        }

        public virtual IStatSessionSummary SummariseSession(IStatSessionSummary baseline)
        {
            var session = this;
            return SummariseSession(session, baseline);
        }
        internal IStatSessionSummary SummariseSession(IStatSession session, IStatSessionSummary baseline)
        {
            // FIXME: Jankness
            var ret = (StatSessionSummary)baseline;
            var summarySection = new StatSessionSummarySection("Session summary");
            summarySection.Add("Session type", session.SessionType);
            summarySection.Add("Commander", session.SessionCommanderName);
            summarySection.Add("Ship", session.SessionShipName);
            summarySection.Add("Start System", session.LocationStart.SystemName);
            summarySection.Add("Start System body", session.LocationStart.BodyName);
            summarySection.Add("End System", session.LocationEnd.SystemName);
            summarySection.Add("End System body", session.LocationEnd.SystemName);

            summarySection.Add("Session started", session.SessionStarted);
            summarySection.Add("Session finished", session.SessionEnded);
            summarySection.Add("Session consumed", session.SessionTimeConsumed);
            summarySection.Add("Session minutes", (int)session.SessionTimeConsumed.TotalMinutes);
            ret.Add(summarySection);

            var totalsSection = new StatSessionSummarySection("Session totals");
            foreach (var item in session.SummaryStats)
                totalsSection.Add(item.Key, item.Value);
            ret.Add(totalsSection);

            var totalMinutes = session.SessionTimeConsumed.TotalMinutes;
            var averagesSection = new StatSessionSummarySection("Session averages");
            foreach (var item in session.SummaryStats)
                averagesSection.Add(item.Key + "(/m)", item.Value / totalMinutes);

            ret.Add(averagesSection);

            return ret;
        }

        public virtual void UpdateLocation(CommanderTravelLocation currentLocation)
        {
            // Nothing to see here
            if (currentLocation == null || string.IsNullOrEmpty(currentLocation.SystemName))
                return;

            LocationCurrent = new CommanderTravelLocation()
            {
                SystemName = currentLocation.SystemName,
                SystemAddress = currentLocation.SystemAddress,
                BodyName = currentLocation.BodyName,
                MarketId = currentLocation.MarketId,
            };

            if (LocationStart == null)
            {
                LocationStart = new CommanderTravelLocation()
                {
                    SystemName = currentLocation.SystemName,
                    SystemAddress = currentLocation.SystemAddress,
                    BodyName = currentLocation.BodyName,
                    MarketId = currentLocation.MarketId,
                };
            }
        }

        protected virtual void UpdateEndLocation(CommanderTravelLocation currentLocation)
        {
            if (LocationEnd == null)
            {
                LocationEnd = new CommanderTravelLocation()
                {
                    SystemName = LocationCurrent.SystemName,
                    BodyName = LocationCurrent.BodyName,
                    MarketId = LocationCurrent.MarketId,
                };
            }
        }
    }
}