namespace TicketVoidingSystem.Models.JackTimeHybridParlay;

public class HybridParlayTicketModel
{
    public string MainTransId { get; set; }
    public int CustId { get; set; }
    public string SubTransId { get; set; }
    public int SubMatchId { get; set; }
    public string SubBetTeam { get; set; }
    public int SubBetTypeId { get; set; }
    public decimal SubHdp1 { get; set; }
    public decimal SubHdp2 { get; set; }
    public string SubBetCheck { get; set; }
    public string Transdesc { get; set; }
    public int SelectionCount { get; set; }
}