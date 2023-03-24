namespace TicketVoidingSystem.Models;

public class NetPositionRequestModel
{
    public string Action { get; set; }
    public long[] TransId { get; set; }
}