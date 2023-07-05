namespace TicketVoidingSystem.Constants;

public static class TicketStatus
{
    public const int RunningId = 0;
    public const int WaitingId = 1;
    public const int WonId = 112;
    public const int LoseId = 113;
    public const int DrawId = 111;
    public const int VoidId = 102;

    public const string Running = "running";
    public const string Void = "void";
    public const string Draw = "draw";
    public const string Won = "won";
    public const string Lose = "lose";
    public const string Sold = "sold";
    public const string Waiting = "waiting";
}