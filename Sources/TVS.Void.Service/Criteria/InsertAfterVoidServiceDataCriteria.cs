using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class InsertAfterVoidServiceDataCriteria : CriteriaBase
{
    public bool IsAutoVoid { get; set; }
    public int RequestId { get; set; }
    public int VoidingHistoryId { get; set; }
    public int MatchId { get; set; }
    public string NegativeBalanceUsers { get; set; }

    public override string GetSettingKey() => "TicketVoiding_VoidBetAutomation_Service_Insert";

    public override bool IsValid() => true;
}