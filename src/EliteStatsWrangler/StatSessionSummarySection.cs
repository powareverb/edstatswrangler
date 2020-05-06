using System.Collections.Generic;
using System;
using System.Globalization;

namespace EliteStatsWrangler
{
    public class StatSessionSummarySection
    {
        private Dictionary<string, string> items = new Dictionary<string, string>();

        public StatSessionSummarySection(string v)
        {
            this.Header = v;
        }

        public string Header { get; internal set; }
        public IDictionary<string, string> Items { get => items; }

        internal void Add(string key, string value)
        {
            items.Add(key, value);
        }

        internal void Add(string key, int value)
        {
            items.Add(key, value.ToString());
        }
        internal void Add(string key, decimal value)
        {
            items.Add(key, value.ToString());
        }
        internal void Add(string key, double value)
        {
            items.Add(key, value.ToString("F2"));
        }
        internal void Add(string key, long value)
        {
            items.Add(key, value.ToString());
        }

        internal void Add(string key, DateTime? value)
        {
            if (value.HasValue)
            {
                string tempValue = value.Value.ToString(CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern);
                items.Add(key, tempValue);
            }
            else
            {
                items.Add(key, "In progress");
            }
        }
        internal void Add(string key, TimeSpan value)
        {
            NodaTime.Duration d = NodaTime.Duration.FromTimeSpan(value);
            items.Add(key, d.ToString());
        }
    }
}