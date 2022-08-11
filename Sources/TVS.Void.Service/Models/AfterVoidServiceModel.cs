namespace TVS.Void.Service.Models
{
    public class AfterVoidServiceModel
    {
        public int SessionId { get; set; }
        public int MatchId { get; set; }
        public int RequestId { get; set; }
        public int VoidingHistoryId { get; set; }
        public int StatusId { get; set; }
    }
}