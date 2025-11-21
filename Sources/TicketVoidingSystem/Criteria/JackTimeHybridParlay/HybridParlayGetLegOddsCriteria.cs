using System;
using Fanex.Data.Repository;

namespace TicketVoidingSystem.Criteria.JackTimeHybridParlay;

public class HybridParlayGetLegOddsCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public string TransIds { get; set; }

    public DateTime LogDate { get; set; }

    public override string GetSettingKey() => "NAP_Admin_HybridParlayLog_Sel";

    public override bool IsValid() => MatchId > 0;
}