using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class GetRequestCriteria : CriteriaBase
    {
        public int RequestId { get; set; }

        public override string GetSettingKey() => "TicketVoiding_Request_Select";

        public override bool IsValid() => true;
    }
}