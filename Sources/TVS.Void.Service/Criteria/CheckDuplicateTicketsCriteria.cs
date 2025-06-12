using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class CheckDuplicateTicketsCriteria : CriteriaBase
{
    public string TransInfoJson { get; set; }

    public string OnlineServerId { get; set; }

    public override string GetSettingKey() => "TicketVoiding_Request_CheckDuplicateTickets";

    public override bool IsValid() => true;
}