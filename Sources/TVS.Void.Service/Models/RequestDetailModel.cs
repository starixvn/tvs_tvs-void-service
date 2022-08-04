namespace TVS.Void.Service.Models
{
    public class RequestDetailModel
    {
        public int RequestId { get; set; }
    }

    public class RequestDetailTicketModel
    {
        public int SiteDbGroupId { get; set; }
        public int TransId { get; set; }
        public int MatchId { get; set; }
        public int MainTransId { get; set; }
    }
}