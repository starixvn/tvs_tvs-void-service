using System.Collections.Generic;
using System.Linq;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void;

public interface InsertAfterVoidDataPrivateService
{
    int GetMatchIdForAfterVoid(int issueTypeId, IEnumerable<RequestDetail> requestDetails,
        IEnumerable<TicketVoidingQueueModel> voidQueueResults);

    string CombineNegativeBalanceUsers(IEnumerable<TicketVoidingQueueModel> voidQueueResults);
}

public class InsertAfterVoidDataPrivateServiceImp : InsertAfterVoidDataPrivateService
{
    public int GetMatchIdForAfterVoid(int issueTypeId, IEnumerable<RequestDetail> requestDetails, IEnumerable<TicketVoidingQueueModel> voidQueueResults)
    {
        var distinctMatchIdInRequest = requestDetails.Select(x => x.MatchId).Distinct();
        var distinctMatchIdInVoidQueues = voidQueueResults.Select(x => x.MatchId).Distinct();
        const int parlayMatchId = 29;
        if (issueTypeId == IssueTypeConstant.AbnormalAccount
            || (issueTypeId == IssueTypeConstant.AbnormalBet && distinctMatchIdInRequest.Any(x => x == parlayMatchId))
            || distinctMatchIdInVoidQueues.Count() > 1)

        {
            return 0;
        }

        return distinctMatchIdInVoidQueues.First();
    }

    public string CombineNegativeBalanceUsers(IEnumerable<TicketVoidingQueueModel> voidQueueResults)
    {
        var validNegativeBalance = voidQueueResults.Where(x => !string.IsNullOrWhiteSpace(x.NegativeBalanceUsers))
            .Select(x => x.NegativeBalanceUsers).Distinct();

        return string.Join(",", validNegativeBalance);
    }
}