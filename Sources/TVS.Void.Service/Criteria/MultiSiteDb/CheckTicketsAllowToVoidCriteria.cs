using System.Collections.Generic;
using System.Linq;
using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;

namespace TVS.Void.Service.Criteria.MultiSiteDb
{
    public class CheckTicketsAllowToVoidCriteria : CriteriaBase
    {
        public string Action { get; set; }
        public string SingleTicketsXml { get; set; }
        public string ParlayTicketsXml { get; set; }

        [SpParam(Ignored = true)]
        public int SiteDbGroupId { get; set; }

        public override string GetSettingKey() => SpList.First(x => x.Id == SiteDbGroupId).SpName;

        public override bool IsValid() => true;

        private static readonly IEnumerable<SiteDbGroupModel> SpList = new List<SiteDbGroupModel>
        {
            new () {Id = SiteDbGroupConstant.CentralDb, SpName = "DBCTG.TicketVoiding_AutoVoidRequest_Validation"},
            new () {Id = SiteDbGroupConstant.SiteDb1, SpName = "DBSIG.TicketVoiding_AutoVoidRequest_Validation"}
        };
    }
}