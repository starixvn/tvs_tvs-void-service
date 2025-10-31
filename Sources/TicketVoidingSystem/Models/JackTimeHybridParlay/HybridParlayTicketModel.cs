namespace TicketVoidingSystem.Models.JackTimeHybridParlay;

public class HybridParlayTicketModel
{
    public string MainTransId { get; set; }
    public int CustId { get; set; }
    public string SubTransId { get; set; }
    public int SubMatchId { get; set; }
    public string SubBetTeam { get; set; }
    public string SubBetTypeId { get; set; }
    public string SubHdp1 { get; set; }
    public string Transdesc { get; set; }
}