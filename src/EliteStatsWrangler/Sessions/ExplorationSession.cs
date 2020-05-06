using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteStatsWrangler.Sessions
{
    public class ExplorationSession : StatSession, IStatSession
    {
        public static string DefaultSessionType = "Exploration";
        public ExplorationSession()//AutoMapper.IMapper objectMapper) : base(objectMapper)
        {
            SessionType = DefaultSessionType;
        }
    }
}
