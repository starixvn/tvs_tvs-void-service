using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

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

        public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "TicketVoiding_Queue_Insert");

        public override bool IsValid() => true;
    }
}