using System.Collections.Generic;
using System.Linq;
using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;

namespace TVS.Void.Service.Criteria.MultiSiteDb
{
    public class UpdateAutoVoidRequestQueueCriteria : NonQueryCommand
    {
        public int QueueId { get; set; }
        public string BroadcastSessionId { get; set; }
        public int? StatusId { get; set; }
        public bool? IsUpdateStartTime { get; set; }
        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public int SiteDbGroupId { get; set; }

        public override string GetSettingKey() => SpList.First(x => x.Id == SiteDbGroupId).SpName;

        public override bool IsValid() => true;

        private static readonly IEnumerable<SiteDbGroupModel> SpList = new List<SiteDbGroupModel>
        {
            new () {Id = SiteDbGroupConstant.CentralDb, SpName = "DBCTG.TicketVoiding_AutoVoidRequest_Queue_Update"},
            new () {Id = SiteDbGroupConstant.SiteDb1, SpName = "DBSIG.TicketVoiding_AutoVoidRequest_Queue_Update"}
        };
    }
}