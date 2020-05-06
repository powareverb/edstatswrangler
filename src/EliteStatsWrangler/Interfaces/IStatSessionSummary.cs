using System.Collections.Generic;
using System;

namespace EliteStatsWrangler
{
    public interface IStatSessionSummary
    {
        DateTime SessionStarted { get; }
        DateTime? SessionEnded { get; }
        TimeSpan SessionTimeConsumed { get; }
        string SessionType { get; }
        string SessionCommanderName { get; }
        CommanderTravelLocation LocationStart { get; } // set;
        CommanderTravelLocation LocationEnd { get; } // set;

        string SessionShipName { get; }
        string SessionShipIdent { get; }

        string TimeStamp { get; }
        IEnumerable<StatSessionSummarySection> Sections { get; }
    }
}