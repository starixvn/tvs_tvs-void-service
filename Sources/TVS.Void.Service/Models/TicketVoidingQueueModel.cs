namespace TVS.Void.Service.Models
{
    public class TicketVoidingQueueModel
    {
        public int QueueId { get; set; }
        public int StatusId { get; set; }
        public int VoidingHistoryId { get; set; }
        public int RequestId { get; set; }
        public int MatchId { get; set; }
    }

    public class ProcessVoidQueueResult
    {
        public int QueueId { get; set; }
        public int StatusId { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}