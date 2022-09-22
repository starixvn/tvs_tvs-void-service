using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class InsertVoidingHistoryCriteria : CriteriaBase
    {
        public string BroadcastSessionId { get; set; }

        public bool IsAutoVoid { get; set; }

        public int UserId { get; set; }

        public string VoidServiceXml { get; set; }

        public override string GetSettingKey() => "TicketVoiding_VoidingHistory_Insert";

        public override bool IsValid() => true;
    }
}