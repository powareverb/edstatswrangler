namespace EliteStatsWrangler
{
    public class ScavengingSession : StatSession, IStatSession
    {
        public static string DefaultSessionType = "Scavenging";
        public ScavengingSession()
        {
            SessionType = DefaultSessionType;
        }
    }
}