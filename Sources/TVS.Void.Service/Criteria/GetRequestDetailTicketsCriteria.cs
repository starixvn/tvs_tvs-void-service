using Fanex.Data.Repository;

namespace TVS.Void.Service.Criteria
{
    public class GetRequestDetailTicketsCriteria : CriteriaBase
    {
        public int SiteDbGroupId { get; set; }

        public int MatchId { get; set; }

        public override string GetSettingKey() => "TicketVoiding_RequestDetailTickets_Select";

        public override bool IsValid() => true;
    }
}