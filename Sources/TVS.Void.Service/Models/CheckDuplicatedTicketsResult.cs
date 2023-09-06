namespace TVS.Void.Service.Models;

public class CheckDuplicatedTicketsResult
{
    public int MatchId { get; set; }
    public int SportType { get; set; }
    public string TransInfoJson { get; set; }
    public string MarkTransIds { get; set; }
    public string UnmarkTransIds { get; set; }
    public string SiteDBAffected { get; set; }
}