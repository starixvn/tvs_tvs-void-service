namespace TVS.Void.Service.Models;

public class AutoVoidRequestQueueModel
{
    public string RequestIds { get; set; }
    public string Action { get; set; }
    public int StatusId { get; set; }
    public int QueueId { get; set; }
    public int SiteDbGroupId { get; set; }
    public string BroadcastSessionId { get; set; }

    //// Use for request VoidInstant with multiple matches, Auto Void Winning with multiple matches
    public int MatchId { get; set; }
}