using AutoMapper;
using System.Collections.Generic;

namespace EliteStatsWrangler
{
    public class StatSessionSummary : StatSession, IStatSessionSummary
    {
        private List<StatSessionSummarySection> _sections = new List<StatSessionSummarySection>();
        public IEnumerable<StatSessionSummarySection> Sections { get { return _sections; } }

        internal void Add(StatSessionSummarySection summarySection)
        {
            _sections.Add(summarySection);
        }
    }
}