namespace TVS.Void.Service.Models
{
    public class AutoVoidRequestQueueModel
    {
        public string BroadcastSessionId { get; set; }
        public int QueueId { get; set; }
        public int RequestId { get; set; }
        public int MatchId { get; set; }
        public int StatusId { get; set; }
    }
}