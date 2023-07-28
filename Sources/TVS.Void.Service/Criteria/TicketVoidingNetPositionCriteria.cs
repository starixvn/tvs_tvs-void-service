using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria;

public class TicketVoidingNetPositionCriteria : NonQueryCommand
{
    public string StrTransIds { get; set; }

    public string Action { get; set; }

    [SpParam(Ignored = true)]
    public int SiteDbGroupId { get; set; }

    public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "NAP_TicketVoiding_NetPosition");

    public override bool IsValid() => true;
}