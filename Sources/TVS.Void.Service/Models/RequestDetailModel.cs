namespace TVS.Void.Service.Models
{
    public class RequestDetailModel
    {
        public int RequestId { get; set; }
    }

    public class RequestDetailTicketModel
    {
        public int RequestId { get; set; }
        public int IssueTypeId { get; set; }
        public int SiteDbGroupId { get; set; }
        public long TransId { get; set; }
        public int MatchId { get; set; }
        public int MainTransId { get; set; }
    }
}