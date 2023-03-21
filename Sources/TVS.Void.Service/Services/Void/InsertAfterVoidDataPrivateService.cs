using System.Collections.Generic;
using System.Linq;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void;

public interface InsertAfterVoidDataPrivateService
{
    bool IsAutoVoid(RequestModel request, VoidingHistoryModel voidDataOfVoidingHistory);

    int GetMatchIdForAfterVoid(int issueTypeId, IEnumerable<RequestDetail> requestDetails,
        IEnumerable<TicketVoidingQueueModel> voidQueueResults);

    string CombineNegativeBalanceUsers(IEnumerable<TicketVoidingQueueModel> voidQueueResults);
}

public class InsertAfterVoidDataPrivateServiceImp : InsertAfterVoidDataPrivateService
{
    public bool IsAutoVoid(RequestModel request, VoidingHistoryModel voidDataOfVoidingHistory)
    {
        var isAutoVoidWinning = request.ActionId is RequestActionConstant.AutoVoidWinning;
        var isAutoVoidInstant = request.ActionId is RequestActionConstant.AutoVoidInstant or RequestActionConstant.AutoVoidInstant5m
            or RequestActionConstant.AutoVoidInstant10m or RequestActionConstant.AutoVoidInstantNoPolicy;

        return (isAutoVoidWinning || isAutoVoidInstant) && voidDataOfVoidingHistory == null;
    }

    public int GetMatchIdForAfterVoid(int issueTypeId, IEnumerable<RequestDetail> requestDetails, IEnumerable<TicketVoidingQueueModel> voidQueueResults)
    {
        var distinctMatchIdInRequest = requestDetails.Select(x => x.MatchId).Distinct();
        var distinctMatchIdInVoidQueues = voidQueueResults.Select(x => x.MatchId).Distinct();
        const int parlayMatchId = 29;
        if (issueTypeId == IssueTypeConstant.AbnormalAccount
            || (issueTypeId == IssueTypeConstant.AbnormalBet && distinctMatchIdInRequest.Any(x => x == parlayMatchId))
            || (distinctMatchIdInVoidQueues.Count() > 1))

        {
            return 0;
        }

        return distinctMatchIdInVoidQueues.First();
    }

    public string CombineNegativeBalanceUsers(IEnumerable<TicketVoidingQueueModel> voidQueueResults)
    {
        var validNegativeBalance = voidQueueResults.Where(x => !string.IsNullOrWhiteSpace(x.NegativeBalanceUsers))
            .Select(x => x.NegativeBalanceUsers);

        return string.Join(",", validNegativeBalance);
    }
}