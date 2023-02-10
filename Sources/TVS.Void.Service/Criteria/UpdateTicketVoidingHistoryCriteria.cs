using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class UpdateTicketVoidingHistoryCriteria : NonQueryCommand
{
    public int VoidingHistoryId { get; set; }
    public int? StatusId { get; set; }
    public int? MessageId { get; set; }
    public int? ReasonId { get; set; }

    public override string GetSettingKey() => "TicketVoiding_VoidingHistory_Update";

    public override bool IsValid() => true;
}