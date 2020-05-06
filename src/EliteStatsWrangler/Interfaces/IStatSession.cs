using System.Collections.Generic;
using System;

namespace EliteStatsWrangler
{
    public interface IStatSession
    {
        DateTime SessionStarted { get; }
        DateTime? SessionEnded { get; }
        TimeSpan SessionTimeConsumed { get; }
        string SessionType { get; }
        string SessionCommanderName { get; set; }
        CommanderTravelLocation LocationCurrent { get; }
        CommanderTravelLocation LocationStart { get; } 
        CommanderTravelLocation LocationEnd { get; } 

        string SessionShipName { get; set; }
        string SessionShipIdent { get; set; }

        Dictionary<string, long> SummaryStats { get; }

        Dictionary<string, List<double>> DoubleValueStats { get; }
        string TimeStamp { get; }
        string ReasonStarted { get; }
        string ReasonEnded { get; }

        void EndSession(DateTime timestamp, string reason);
        void StartSession(DateTime timestamp, string reason);
        void SetSessionDefaults(ActiveSessions activeSessions);
        IStatSessionSummary SummariseSession(IStatSessionSummary baseline);
        void UpdateLocation(CommanderTravelLocation currentLocation);
    }
}