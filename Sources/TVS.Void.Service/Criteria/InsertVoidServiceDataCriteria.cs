using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class InsertVoidServiceDataCriteria : CriteriaBase
    {
        public string BroadcastSessionId { get; set; }
        public int SiteDbGroupId { get; set; }
        public bool IsAutoVoid { get; set; }

        public string RequestInfoXml { get; set; }

        public override string GetSettingKey() => "TicketVoiding_Void_Service_Insert";

        public override bool IsValid() => true;
    }
}