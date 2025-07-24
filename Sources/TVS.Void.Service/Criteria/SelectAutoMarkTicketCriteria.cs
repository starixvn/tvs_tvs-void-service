using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class SelectAutoMarkTicketCriteria : CriteriaBase
{
    public override string GetSettingKey() => "TicketVoiding_PendingMarkTickets_Select";

    public override bool IsValid() => true;
}