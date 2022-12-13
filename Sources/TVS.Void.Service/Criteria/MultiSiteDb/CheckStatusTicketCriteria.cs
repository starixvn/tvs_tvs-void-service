using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria.MultiSiteDb
{
    public class CheckStatusTicketCriteria : CriteriaBase
    {
        public string SingleTicketsJson { get; set; }
        public string ParlayTicketsJson { get; set; }

        [SpParam(Ignored = true)]
        public int SiteDbGroupId { get; set; }

        public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "TicketVoiding_AutoVoidRequest_CheckQueue");

        public override bool IsValid() => true;
    }
}