using System;

namespace EliteStatsWrangler
{
    public class TradingSession : StatSession, IStatSession
    {
        public static string DefaultSessionType = "Trading";
        public TradingSession()//AutoMapper.IMapper objectMapper) : base(objectMapper)
        {
            SessionType = DefaultSessionType;
        }

        public void UpdatedTrade(DateTime timestamp, long marketId, string tradeType, string typeLocalised, long count, long price)
        {
            var tradingSession = this;
            tradingSession.IncrementStat($"Trading - {tradeType}", count);
            tradingSession.IncrementStat($"Trading - {tradeType} - {typeLocalised}", count);
            tradingSession.IncrementStat($"Trading - {tradeType} - {typeLocalised} - Market:{marketId}", count);

            var signPrice = price;
            if (tradeType.Equals("buy", StringComparison.OrdinalIgnoreCase))
                signPrice *= -1;

            // This should automagically subtract buy from sale
            tradingSession.IncrementStat($"Traded Profit", signPrice * count);
            tradingSession.IncrementStat($"Traded Profit - {typeLocalised}", signPrice * count);

            tradingSession.ValueStat($"Trading - {tradeType}", count);
            tradingSession.ValueStat($"Trading - {tradeType} - {typeLocalised}", count);
            tradingSession.ValueStat($"TradedPrice - {tradeType} - {typeLocalised}", price);
        }

    }
}