using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria.MultiSiteDb
{
    public class CheckTicketsAllowToVoidCriteria : CriteriaBase
    {
        public string Action { get; set; }
        public string SingleTicketsXml { get; set; }
        public string ParlayTicketsXml { get; set; }

        [SpParam(Ignored = true)]
        public int SiteDbGroupId { get; set; }

        public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "TicketVoiding_AutoVoidRequest_Validation");

        public override bool IsValid() => true;
    }
}