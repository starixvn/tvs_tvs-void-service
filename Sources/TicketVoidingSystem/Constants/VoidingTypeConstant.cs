namespace TicketVoidingSystem.Constants;

public static class VoidingTypeConstant
{
    public const byte Single = 1;

    public const byte Parlay = 2;

    public const byte CancelCashOut = 3;

    public const byte Cricket = 4;

    public const byte Single14 = 5;

    public const byte Parlay14 = 6;

    public const byte HybridParlay = 18;

    public const byte SubHybridParlay = 20;

    public const byte MainHybridParlay14 = 22;

    /// <summary>
    /// RECOVER
    /// </summary>

    public const byte RecoverSingle = 11;

    public const byte RecoverParlay = 12;

    public const byte RecoverSubParlay = 13;

    public const byte RecoverCricket = 14;

    public const byte RecoverSingle14 = 15;

    public const byte RecoverParlay14 = 16;

    public const byte RecoverHybridParlay = 19;

    public const byte RecoverSubHybridParlay = 21;

    public const byte RecoverMainHybridParlay14 = 23;

    public static readonly byte[] VoidingTypeRecover = [
        RecoverSingle,
        RecoverParlay,
        RecoverSubParlay,
        RecoverCricket,
        RecoverSingle14,
        RecoverParlay14,
        RecoverHybridParlay,
        RecoverSubHybridParlay
    ];
}