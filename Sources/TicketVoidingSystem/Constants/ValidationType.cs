namespace TicketVoidingSystem.Constants;

public static class ValidationType
{
    // VOID

    public const int SingleTicketVoid = 1;
    public const int SingleTicketVoidResettle = 2;
    public const int ParlayTicketVoid = 3;
    public const int ParlayTicketVoidResettle = 4;
    public const int CricketTicketVoid = 5;
    public const int CricketTicketVoidResettle = 6;
    public const int CancelCashOutTicketVoid = 7;
    public const int SingleTicket14Void = 8;
    public const int ParlayTicket14Void = 9;
    public const int HybridParlayMainTicketVoid = 10;
    public const int HybridParlayMainTicketResettleVoid = 11;
    public const int HybridParlaySubTicketVoid = 12;
    public const int HybridParlayMainTicket14Void = 13;

    // RECOVER

    public const int SingleTicketRecover = 21;
    public const int SingleTicketResettle = 22;
    public const int ParlayTicketRecover = 23;
    public const int ParlayTicketResettle = 24;
    public const int SubParlayTicketRecover = 25;
    public const int SubParlayTicketResettle = 26;
    public const int CricketTicketRecover = 27;
    public const int CricketTicketResettle = 28;
    public const int SingleTickets14Recover = 29;
    public const int ParlayTickets14Recover = 30;
    public const int HybridParlaySubTicketRecover = 31;
    public const int HybridParlaySubTicketResettle = 32;
    public const int HybridParlayMainTicketRecover = 33;
    public const int HybridParlayMainTicketResettle = 34;
    public const int HybridParlayMainTicket14Resettle = 35;
}