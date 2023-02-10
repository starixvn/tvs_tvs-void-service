using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria.MultiSiteDb;

public class UnMarkTicketsCriteria : CriteriaBase
{
    public string TransIds { get; set; }
    public int UserId { get; set; }
    public string Ip { get; set; }

    public int LogActionId { get; set; }

    [SpParam(Ignored = true)]
    public int SiteDbGroupId { get; set; }

    public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "Nap_SpecialDelete_MarkedTickets_Update");

    public override bool IsValid() => true;
}