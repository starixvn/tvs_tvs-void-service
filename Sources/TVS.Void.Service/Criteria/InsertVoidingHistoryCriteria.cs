using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class InsertVoidingHistoryCriteria : CriteriaBase
    {
        public int RequestId { get; set; }

        public int UserId { get; set; }

        public int SiteDbGroupId { get; set; }

        public string BroadcastSessionId { get; set; }

        public override string GetSettingKey() => "TicketVoiding_VoidingHistory_Insert";

        public override bool IsValid() => true;
    }
}