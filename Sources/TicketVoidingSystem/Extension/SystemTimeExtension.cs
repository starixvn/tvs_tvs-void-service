using System;

namespace TicketVoidingSystem.Extension;

public static class SystemTimeExtension
{
    private const sbyte OffsetHour = 12;

    public static DateTime ZoneManila(this DateTime dateTime)
    {
        return Convert.ToDateTime(dateTime).AddHours(OffsetHour);
    }
}