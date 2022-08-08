using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fanex.Data;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Criteria.MultiSiteDb
{
    public class InsertTicketVoidingQueueCriteria : NonQueryCommand
    {
        public int RequestId { get; set; }
        public int VoidingHistoryId { get; set; }
        public int IssueTypeId { get; set; }
        public string SingleTicketsXml { get; set; }
        public string ParlayTicketsXml { get; set; }
        public int UserId { get; set; }
        public string IP { get; set; }

        [SpParam(Ignored = true)]
        public int SiteDbGroupId { get; set; }

        public override string GetSettingKey() => SpList.First(x => x.Id == SiteDbGroupId).SpName;

        public override bool IsValid() => true;

        private static readonly IEnumerable<SiteDbGroupModel> SpList = new List<SiteDbGroupModel>
        {
            new () {Id = SiteDbGroupConstant.CentralDb, SpName = "CentralDb.TicketVoiding_Queue_Insert"},
            new () {Id = SiteDbGroupConstant.SiteDb1, SpName = "SiteDb1.TicketVoiding_Queue_Insert"}
        };
    }
}