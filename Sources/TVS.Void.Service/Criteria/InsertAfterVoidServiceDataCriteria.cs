using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class InsertAfterVoidServiceDataCriteria : CriteriaBase
    {
        public bool IsAutoVoid { get; set; }

        public string AfterVoidDataXml { get; set; }

        public override string GetSettingKey() => "TicketVoiding_VoidBetAutomation_Service_Insert";

        public override bool IsValid() => true;
    }
}