using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class InsertVoidingHistoryCriteria : CriteriaBase
{
    public string RequestsJson { get; set; }

    public int UserId { get; set; }

    public int SiteDbGroupId { get; set; }

    public string BroadcastSessionId { get; set; }

    public string VoidData { get; set; }

    public override string GetSettingKey() => "TicketVoiding_VoidingHistory_Insert";

    public override bool IsValid() => true;
}