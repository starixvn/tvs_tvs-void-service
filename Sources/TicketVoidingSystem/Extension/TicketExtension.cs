using System.Collections.Generic;
using TicketVoidingSystem.Constants;

namespace TicketVoidingSystem.Extension;

public static class TicketExtension
{
    private static readonly List<int> SettledStatus = [TicketStatus.DrawId, TicketStatus.WonId, TicketStatus.LoseId];

    public static bool IsSettled(this int statusId)
    {
        return SettledStatus.Contains(statusId);
    }

    public static bool IsCashOut(this int betdaqId)
    {
        return betdaqId == -2;
    }
}