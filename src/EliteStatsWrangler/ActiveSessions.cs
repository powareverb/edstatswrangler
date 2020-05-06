
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using AutoMapper;
using EliteStatsWrangler.Sessions;
using System.Linq;

namespace EliteStatsWrangler
{
    public class ActiveSessions
    {
        private static List<IStatSession> sessionList = new List<IStatSession>();
        // FIXME: Jank
        public IMapper ObjectMapper { get; internal set; }
        private readonly string _basePath;

        public ActiveSessions(string storageLocation)
        {
            this._basePath = storageLocation;
        }

        // TODO: Invert?
        public List<IStatSession> CurrentSessions { get { 
                return new List<IStatSession>(new IStatSession[] {
                    currentMiningSession,
                    currentTradingSession,
                    currentCombatSession,
                    currentTravelSession,
                    currentExplorationSession,
                }); 
            } 
        }
        private MiningSession currentMiningSession = null;
        private TradingSession currentTradingSession = null;
        private CombatSession currentCombatSession = null;
        private TravelSession currentTravelSession = null;
        private ExplorationSession currentExplorationSession = null; // TODO: Implement

        public bool IsActivelyMining { get { return currentMiningSession != null; } }
        public bool IsActivelyTrading { get { return currentTradingSession != null; } }
        public bool IsActivelyFighting { get { return currentCombatSession != null; } }
        public bool IsActivelyTravelling { get { return currentTravelSession != null; } }

        public string CurrentShip { get; internal set; }
        public string CommanderName { get; internal set; }
        public string CurrentShipIdent { get; internal set; }
        public CommanderTravelLocation CurrentLocation { get; private set; }

        /// <summary>
        /// Start a mining session if one doesn't exist, or continue existing
        /// </summary>
        /// <returns></returns>
        internal MiningSession GetMiningSession(DateTime timestamp, string reason)
        {
            if (!IsActivelyMining)
            {
                currentMiningSession = new MiningSession();
                currentMiningSession.SetSessionDefaults(this);
                currentMiningSession.StartSession(timestamp, reason);
            }
            return currentMiningSession;
        }

        internal TradingSession GetTradeSession(DateTime timestamp, string reason)
        {
            if (!IsActivelyTrading)
            {
                currentTradingSession = new TradingSession();
                currentTradingSession.SetSessionDefaults(this);
                currentTradingSession.StartSession(timestamp, reason);
            }
            return currentTradingSession;
        }

        internal TravelSession GetTravelSession(DateTime timestamp, string reason)
        {
            if (!IsActivelyTravelling)
            {
                currentTravelSession = new TravelSession();
                currentTravelSession.SetSessionDefaults(this);
                currentTravelSession.StartSession(timestamp, reason);
            }
            return currentTravelSession;
        }

        internal CombatSession GetCombatSession(DateTime timestamp, string reason)
        {
            if (!IsActivelyFighting)
            {
                currentCombatSession = new CombatSession();
                currentCombatSession.SetSessionDefaults(this);
                currentCombatSession.StartSession(timestamp, reason);
            }
            return currentCombatSession;
        }

        internal void CompleteSession(IStatSession session, DateTime timestamp, string reason)
        {
            // Double check session has some stuff that may have been missed - note this might not be accurate as of start of session :/
            session.SetSessionDefaults(this);
            session.EndSession(timestamp, reason);

            // TODO: Deal with previous summaries?

            if(session.SessionTimeConsumed < TimeSpan.FromMinutes(1))
            {
                // Bundle these away for debugging?
                CloseSession(session);
                return;
            }

            sessionList.Add(session);
            // FIXME: Still Jank
            var baseline = ObjectMapper.Map<StatSessionSummary>(session);
            var summary = session.SummariseSession(baseline);
            DisplaySummary(summary);
            SaveSession(session);
            SaveSessionSummary(summary);

            CloseSession(session);
        }

        private void CloseSession(IStatSession session)
        {
            // Close off session based on type
            if (session.SessionType == MiningSession.DefaultSessionType)
                currentMiningSession = null;
            if (session.SessionType == TradingSession.DefaultSessionType)
                currentTradingSession = null;
            if (session.SessionType == CombatSession.DefaultSessionType)
                currentCombatSession = null;
            if (session.SessionType == TravelSession.DefaultSessionType)
                currentTravelSession = null;
        }

        private void DisplaySummary(IStatSessionSummary summary)
        {
            var divider = "================================";
            var subdivider = "================";

            foreach(var section in summary.Sections)
            {
                Console.WriteLine($"{divider}");
                Console.WriteLine($"{section.Header}");
                Console.WriteLine($"{subdivider}");

                foreach(var item in section.Items)
                {
                    Console.WriteLine($"{item.Key}:\t\t\t{item.Value}");
                }
            }
        }

        public void UpdatedLocation(DateTime timestamp, CommanderTravelLocation location, string reason)
        {
            this.CurrentLocation = location;
            var travelSession = StartedTravelling(timestamp, reason);
            foreach (var currentSession in CurrentSessions.Where(p => p != null))
            {
                currentSession.UpdateLocation(this.CurrentLocation);
            }
        }

        public MiningSession StartedMining(DateTime timestamp, string reason)
        {
            if (!this.IsActivelyMining)
            {
                var session = this.GetMiningSession(timestamp, reason);
                session.ReasonStarted = reason;
                NotifySession(session);
                return session;
            }
            else
            {
                return this.GetMiningSession(timestamp, reason);
            }
        }

        public void StoppedMining(DateTime timestamp, string reason)
        {
            if (this.IsActivelyMining)
                this.CompleteSession(this.GetMiningSession(timestamp, reason), timestamp, reason);
        }

        public CombatSession StartedCombat(DateTime timestamp, string reason)
        {
            if (!this.IsActivelyFighting)
            {
                var session = this.GetCombatSession(timestamp, reason);
                NotifySession(session);
                return session;
            }
            else
            {
                return this.GetCombatSession(timestamp, reason);
            }
        }

        public void StoppedCombat(DateTime timestamp, string reason)
        {
            if (this.IsActivelyFighting)
                this.CompleteSession(this.GetCombatSession(timestamp, reason), timestamp, reason);
        }

        public TravelSession StartedTravelling(DateTime timestamp, string reason)
        {
            if (!this.IsActivelyTravelling)
            {
                var session = this.GetTravelSession(timestamp, reason);
                NotifySession(session);
                return session;
            }
            else
            {
                var sess = this.GetTravelSession(timestamp, reason);
                // Update current status?
                return sess;
            }
        }

        public void StoppedTravelling(DateTime timestamp, string reason)
        {
            if (this.IsActivelyTravelling)
                this.CompleteSession(this.GetTravelSession(timestamp, reason), timestamp, reason);
        }

        public TradingSession StartedTrading(DateTime timestamp, string reason)
        {
            if (!this.IsActivelyTrading)
            {
                var session = this.GetTradeSession(timestamp, reason);
                NotifySession(session);
                return session;
            }
            else
            {
                return this.GetTradeSession(timestamp, reason);
            }
        }

        public void StoppedTrading(DateTime timestamp, string reason)
        {
            if (this.IsActivelyTrading)
                this.CompleteSession(this.GetTradeSession(timestamp, reason), timestamp, reason);
        }

        public void SaveSessionSummary(IStatSessionSummary session)
        {
            using (StreamWriter file = File.CreateText(_basePath + $"/SessionSummary-{session.TimeStamp}-{session.SessionType}.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, (StatSessionSummary)session);
            }
        }

        private void SaveSession(IStatSession session)
        {
            using (StreamWriter file = File.CreateText(_basePath+ $"/Session-{session.TimeStamp}-{session.SessionType}.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, (StatSession)session);
            }
        }

        private static void NotifySession(IStatSession session)
        {
            Console.WriteLine($"Started session: {session.SessionType} in {session.LocationStart?.SystemName}");
        }

        internal void Shutdown(DateTime timestamp)
        {
            var reason = "Shutdown";
            if (this.IsActivelyMining)
                this.CompleteSession(this.GetMiningSession(timestamp, reason), timestamp, reason);

            if (this.IsActivelyFighting)
                this.CompleteSession(this.GetCombatSession(timestamp, reason), timestamp, reason);

            if (this.IsActivelyTrading)
                this.CompleteSession(this.GetTradeSession(timestamp, reason), timestamp, reason);

            if (this.IsActivelyTravelling)
                this.CompleteSession(this.GetTravelSession(timestamp, reason), timestamp, reason);

            // Save all sessions?
            foreach (var session in sessionList)
            {
                // FIXME: Still Jank
                var baseline = ObjectMapper.Map<StatSessionSummary>(session);
                var summary = session.SummariseSession(baseline);
                DisplaySummary(summary);
                SaveSession(session);
                SaveSessionSummary(summary);
            }
        }
    }
}