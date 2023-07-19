namespace TicketVoidingSystem.Constants;

public static class MarkTypeConstant
{
    public const byte ExecuteMarkTickets = 0;
    public const byte ExecuteUnMarkTickets = 1;
}

public static class LogActionIdConstant
{
    public const int MarkTicketSpecialDelete = 90500;
    public const int UnMarkTicketSpecialDelete = 90501;
    public const int MarkTicketAddRequestTvs = 90508;
    public const int UnMarkTicketAddRequestTvs = 90511;
}