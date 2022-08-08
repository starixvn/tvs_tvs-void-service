namespace TVS.Void.Service.Models
{
    public class AutoVoidRequestQueueModel
    {
        public int QueueId { get; set; }
        public int RequestId { get; set; }
        public int MatchId { get; set; }
        public string Action { get; set; }
        public int StatusId { get; set; }
        public string BroadcastSessionId { get; set; }
    }
}