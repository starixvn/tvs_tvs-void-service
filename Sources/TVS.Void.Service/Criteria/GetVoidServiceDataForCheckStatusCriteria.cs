using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class GetVoidServiceDataForCheckStatusCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "TicketVoiding_Void_Service_Select";

        public override bool IsValid() => true;
    }
}