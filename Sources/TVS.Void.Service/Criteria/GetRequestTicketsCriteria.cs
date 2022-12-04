using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class GetRequestTicketsCriteria : CriteriaBase
    {
        public int? SiteDbGroupId { get; set; }

        public int? RequestId { get; set; }

        public override string GetSettingKey() => "TicketVoiding_AutoVoidRequest_Ticket_Select";

        public override bool IsValid() => true;
    }
}