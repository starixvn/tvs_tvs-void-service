using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class GetDuplicatedTicketsCriteria : CriteriaBase
{
    public override string GetSettingKey() => "TicketVoiding_DuplicateTickets_Select";

    public override bool IsValid() => true;
}