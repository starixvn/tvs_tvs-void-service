namespace TVS.Void.Service.Models;

public class TicketVoidingQueueModel
{
    public int QueueId { get; set; }
    public int StatusId { get; set; }
    public int VoidingHistoryId { get; set; }
    public int RequestId { get; set; }
    public int MatchId { get; set; }
    public int VoidingTypeId { get; set; }

    public int UserId { get; set; }
    public string UserName { get; set; }
    public string NegativeBalanceUsers { get; set; }
    public string TransIds { get; set; }
    public string ErrMsg { get; set; }
}