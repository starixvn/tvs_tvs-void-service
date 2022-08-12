namespace TVS.Void.Service.Models
{
    public class VoidServiceModel
    {
        public string BroadcastSessionId { get; set; }
        public int SiteDbGroupId { get; set; }
        public int RequestId { get; set; }
        public int VoidingHistoryId { get; set; }
        public int StatusId { get; set; }
        public bool IsAutoVoid { get; set; }
        public int IssueTypeId { get; set; }
        public int MatchId { get; set; }
    }
}