using TicketVoidingSystem.Constants;

namespace TicketVoidingSystem.Extension;

public static class BetTypeExtension
{
    public static bool IsParlay(this int id)
    {
        return id is BetTypeConstant.MixParlay or BetTypeConstant.VirtualSportParlay or BetTypeConstant.VirtualSport4Parlay;
    }
}