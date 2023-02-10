using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria.MultiSiteDb;

public class ReCalculateTicketsCriteria : NonQueryCommand
{
    public int MatchId { get; set; }
    public string TransIds { get; set; }
    public int UserId { get; set; }
    public string Ip { get; set; }

    [SpParam(Ignored = true)]
    public int SiteDbGroupId { get; set; }

    public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "TicketVoiding_ReCalculate_Single_PreUpdate");

    public override bool IsValid() => true;
}