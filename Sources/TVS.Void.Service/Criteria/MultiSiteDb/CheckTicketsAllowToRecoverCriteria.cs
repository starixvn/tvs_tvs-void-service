using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria.MultiSiteDb;

public class CheckTicketsAllowToRecoverCriteria : CriteriaBase
{
    public string SingleTicketsJson { get; set; }
    public string ParlayTicketsJson { get; set; }
    public string CricketTicketsJson { get; set; }

    [SpParam(Ignored = true)]
    public int SiteDbGroupId { get; set; }

    public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "TicketVoiding_AutoRecover_Validation");

    public override bool IsValid() => true;
}