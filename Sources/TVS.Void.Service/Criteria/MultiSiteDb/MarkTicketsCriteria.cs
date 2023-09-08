using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria.MultiSiteDb;

public class MarkTicketsCriteria : CriteriaBase
{
    public int UserId { get; set; }

    public string Ip { get; set; }

    public int LogActionId { get; set; }

    public byte Status { get; set; }

    public string TransIds { get; set; }

    public int IssueTypeId { get; set; }

    [SpParam(Ignored = true)]
    public int SiteDbGroupId { get; set; }

    public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "Nap_SpecialDelete_MarkTickets_Update");

    public override bool IsValid() => true;
}