using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria.MultiSiteDb
{
    public class ProcessVoidQueueCriteria : CriteriaBase
    {
        public int QueueId { get; set; }

        [SpParam(Ignored = true)]
        public int SiteDbGroupId { get; set; }

        public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "TicketVoiding_Queue_Job");

        public override bool IsValid() => true;
    }
}