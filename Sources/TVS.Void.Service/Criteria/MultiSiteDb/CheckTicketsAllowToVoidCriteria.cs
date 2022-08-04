using System.Collections.Generic;
using System.Linq;
using Fanex.Data;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Criteria.MultiSiteDb
{
    public class CheckTicketsAllowToVoidCriteria : CriteriaBase
    {
        public string SingleTickets { get; set; }
        public string ParlayTickets { get; set; }

        [SpParam(Ignored = true)]
        public int SiteDbGroupId { get; set; }

        public override string GetSettingKey() => SpList.First(x => x.Id == SiteDbGroupId).SpName;

        public override bool IsValid() => true;

        private static readonly IEnumerable<SiteDbGroupModel> SpList = new List<SiteDbGroupModel>
        {
            new () {Id = SiteDbGroupConstant.CentralDb, SpName = "CentralDb.TicketVoiding_CheckTickets"},
            new () {Id = SiteDbGroupConstant.SiteDb1, SpName = "SiteDb1.TicketVoiding_CheckTickets"}
        };
    }
}