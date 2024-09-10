using System.Collections.Generic;

namespace TVS.Void.Service.Constants;

public static class AutoVoidConstant
{
    public const string ActorName = "starixIT";
    public const int ActorId = 10_278_938;
    public const string IPAddress = "125.227.140.97";
    public const string QueueActionVoidInstant = "VoidInstant";
    public const string QueueActionVoidInstantNoPolicy = "VoidInstantNopolicy";
    public const string QueueActionVoidInstantWithPolicy = "VoidInstantPolicy";
    public const string QueueActionVoidResultKeyed = "VoidResultKeyed";
    public const string QueueActionCal = "CAL";
    public const string QueueRecoverInstant = "RecoverInstant";
}

public static class VoidActionTypeConstant
{
    public const int AutoVoidWinningId = 1;
    public const int AutoVoidInstantId = 4;
    public const int AutoVoidInstantNoPolicyId = 8;
    public const int AutoVoidAllResultKeyedId = 9;
    public const int AutoVoidAllInstantWithPolicyId = 10;
    public const int AutoRecoverInstant = 11;

    public static readonly Dictionary<int, string> VoidActionList = new()
    {
        { AutoVoidWinningId, AutoVoidConstant.QueueActionCal },
        { AutoVoidInstantId, AutoVoidConstant.QueueActionVoidInstant },
        { AutoVoidInstantNoPolicyId, AutoVoidConstant.QueueActionVoidInstantNoPolicy },
        { AutoVoidAllResultKeyedId, AutoVoidConstant.QueueActionVoidResultKeyed },
        { AutoVoidAllInstantWithPolicyId, AutoVoidConstant.QueueActionVoidInstantWithPolicy },
        { AutoRecoverInstant, AutoVoidConstant.QueueRecoverInstant }
    };
}