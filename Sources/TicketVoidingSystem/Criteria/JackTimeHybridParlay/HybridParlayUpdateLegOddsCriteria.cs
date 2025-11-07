using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TicketVoidingSystem.Criteria.JackTimeHybridParlay;

public class HybridParlayUpdateLegOddsCriteria : CriteriaBase
{
    public string LegOddsJson { get; set; }

    public int MatchId { get; set; }

    public int Creator { get; set; }

    [SpParam(Ignored = true)]
    public int SiteDbGroupId { get; set; }

    public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "NAP_Admin_HybridParlay_LegOddsUpd");

    public override bool IsValid() => !string.IsNullOrWhiteSpace(LegOddsJson);
}