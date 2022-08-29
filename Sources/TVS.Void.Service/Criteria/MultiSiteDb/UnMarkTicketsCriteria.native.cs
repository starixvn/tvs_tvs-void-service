using System.Collections.Generic;
using System.Linq;
using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;

namespace TVS.Void.Service.Criteria.MultiSiteDb
{
    public class UnMarkTicketsCriteria : CriteriaBase
    {
        public string TransIds { get; set; }
        public int UserId { get; set; }
        public string Ip { get; set; }

        [SpParam(Ignored = true)]
        public int SiteDbGroupId { get; set; }

        public override string GetSettingKey() => SpList.First(x => x.Id == SiteDbGroupId).SpName;

        public override bool IsValid() => true;

        private static readonly IEnumerable<SiteDbGroupModel> SpList = new List<SiteDbGroupModel>
        {
            new () {Id = SiteDbGroupConstant.CentralDb, SpName = "DBCTG.Nap_SpecialDelete_MarkedTickets_Update"},
            new () {Id = SiteDbGroupConstant.SiteDb1, SpName = "DBSIG.Nap_SpecialDelete_MarkedTickets_Update"}
        };
    }
}