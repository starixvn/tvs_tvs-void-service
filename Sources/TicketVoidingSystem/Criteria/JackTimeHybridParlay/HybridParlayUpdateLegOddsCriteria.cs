using Fanex.Data.Repository;

namespace TicketVoidingSystem.Criteria.JackTimeHybridParlay;

public class HybridParlayUpdateLegOddsCriteria : CriteriaBase
{
    public string LegOddsJson { get; set; }

    public override string GetSettingKey() => "NAP_Admin_HybridParlay_LegOddsUpd";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(LegOddsJson);
}