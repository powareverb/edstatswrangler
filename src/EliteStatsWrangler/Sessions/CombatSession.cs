namespace EliteStatsWrangler
{
    public class CombatSession : StatSession, IStatSession
    {
        public static string DefaultSessionType = "Combat";
        public CombatSession()//(AutoMapper.IMapper objectMapper) : base(objectMapper)
        {
            SessionType = DefaultSessionType;
        }
    }
}