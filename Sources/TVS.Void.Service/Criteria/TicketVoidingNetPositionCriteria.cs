using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria;

public class TicketVoidingNetPositionCriteria : NonQueryCommand
{
    public string StrTransIds { get; set; }

    public string Action { get; set; }

    public override string GetSettingKey() => "NAP_TicketVoiding_NetPosition";

    public override bool IsValid() => true;
}