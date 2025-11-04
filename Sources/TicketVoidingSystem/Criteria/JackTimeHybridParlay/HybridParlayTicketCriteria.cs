using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TicketVoidingSystem.Criteria.JackTimeHybridParlay;

public class HybridParlayTicketCriteria : CriteriaBase
{
    public string TransIds { get; set; }

    public int TicketType { get; set; }

    [SpParam(Ignored = true)]
    public int SiteDbGroupId { get; set; }

    public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "TicketVoiding_TicketsDetail_Select");

    public override bool IsValid() => true;
}