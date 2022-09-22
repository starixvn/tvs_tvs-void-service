using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class GetRequestDetailTicketsCriteria : CriteriaBase
    {
        public int SiteDbGroupId { get; set; }

        public int RequestId { get; set; }
        public int MatchId { get; set; }

        public string Action { get; set; }

        public override string GetSettingKey() => "TicketVoiding_AutoVoidRequest_Ticket_Select";

        public override bool IsValid() => true;
    }
}