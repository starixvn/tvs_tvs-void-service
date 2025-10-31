using System.Collections.Generic;

namespace TicketVoidingSystem.Models.JackTimeHybridParlay;

public class HybridParlayApiModel
{
    public IEnumerable<HybridParlayVoidBetInfo> VoidBetInfos { get; set; }
}

public class HybridParlayVoidBetInfo
{
    public string TransactionId { get; set; }
    public string TransId { get; set; }
    public int CustId { get; set; }
    public IEnumerable<LegModel> Legs { get; set; }
}

public class LegModel
{
    public string WayId { get; set; }
    public int MatchId { get; set; }
    public string MarketId { get; set; }
}