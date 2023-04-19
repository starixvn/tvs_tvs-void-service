using Fanex.Data;
using Fanex.Data.Repository;
using TicketVoidingSystem.Utilities;

namespace TVS.Void.Service.Criteria.MultiSiteDb;

public class InsertTicketVoidingQueueCriteria : NonQueryCommand
{
    public int RequestId { get; set; }
    public int VoidingHistoryId { get; set; }
    public string SingleTicketsJson { get; set; }
    public string ParlayTicketsJson { get; set; }
    public string CricketTicketsJson { get; set; }
    public int UserId { get; set; }
    public string IP { get; set; }

    [SpParam(Ignored = true)]
    public int SiteDbGroupId { get; set; }

    public override string GetSettingKey() => Helper.GetSettingKeyBySiteDbGroupId(SiteDbGroupId, "TicketVoiding_Queue_Insert");

    public override bool IsValid() => true;
}