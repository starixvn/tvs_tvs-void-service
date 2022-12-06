using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria.MultiSiteDb
{
    public class UpdateAutoVoidRequestQueueCriteria : NonQueryCommand
    {
        public int QueueId { get; set; }
        public int? StatusId { get; set; }
        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public int SiteDbGroupId { get; set; }

        public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "TicketVoiding_AutoVoidRequest_Queue_Update");

        public override bool IsValid() => true;
    }
}