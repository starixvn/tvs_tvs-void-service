namespace TVS.Void.Service.Models
{
    public class AutoVoidRequestQueueModel
    {
        public int RequestId { get; set; }
        public string Action { get; set; }
        public int StatusId { get; set; }
        public int QueueId { get; set; }
        public int SiteDbGroupId { get; set; }
        public string BroadcastSessionId { get; set; }
    }
}