using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class GetVoidDataOfVoidingHistoryCriteria : CriteriaBase
{
    public string VoidingHistoryIds { get; set; }

    public override string GetSettingKey() => "TicketVoiding_VoidingHistory_Select";

    public override bool IsValid() => true;
}