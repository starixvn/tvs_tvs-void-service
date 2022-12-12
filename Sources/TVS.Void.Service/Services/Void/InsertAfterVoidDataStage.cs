using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void
{
    public interface InsertAfterVoidDataStage
    {
        Task Execute(IEnumerable<TicketVoidingQueueModel> voidQueueResults, IList<string> logData);
    }

    public class InsertAfterVoidDataStageImp : InsertAfterVoidDataStage
    {
        private readonly RequestDetailService requestDetailService;

        public InsertAfterVoidDataStageImp(RequestDetailService requestDetailService)
        {
            this.requestDetailService = requestDetailService;
        }

        public async Task Execute(IEnumerable<TicketVoidingQueueModel> voidQueueResults, IList<string> logData)
        {
            var firstVoidQueue = voidQueueResults.First();
            var (request, requestDetails) = await requestDetailService.GetRequestById(firstVoidQueue.RequestId);
            var afterVoidData = new AfterVoidServiceModel
            {
                RequestId = firstVoidQueue.RequestId,
                VoidingHistoryId = firstVoidQueue.VoidingHistoryId,
                MatchId = GetMatchIdForAfterVoid(firstVoidQueue.VoidingHistoryId, requestDetails, voidQueueResults),
                NegativeBalanceUsers = CombineNegativeBalanceUsers(voidQueueResults)
            };

            var insertedAfterVoidData = await requestDetailService.InsertAfterVoidServiceData(IsAutoVoid(request.First()), afterVoidData);
            logData.Add($"{nameof(RequestDetailService.InsertAfterVoidServiceData)}: {JsonSerializer.Serialize(insertedAfterVoidData)}");
        }

        private static bool IsAutoVoid(RequestModel request)
        {
            var isAutoVoidWinning = request.ActionId is RequestActionConstant.AutoVoidWinning;
            var isAutoVoidInstant = request.ActionId is RequestActionConstant.AutoVoidInstant or RequestActionConstant.AutoVoidInstant5m
                                    or RequestActionConstant.AutoVoidInstant10m or RequestActionConstant.AutoVoidInstantNoPolicy;
            return isAutoVoidWinning || isAutoVoidInstant;
        }

        private static int GetMatchIdForAfterVoid(int issueTypeId, IEnumerable<RequestDetail> requestDetails, IEnumerable<TicketVoidingQueueModel> voidQueueResults)
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

        private static string CombineNegativeBalanceUsers(IEnumerable<TicketVoidingQueueModel> voidQueueResults)
        {
            var validNegativeBalance = voidQueueResults.Where(x => !string.IsNullOrWhiteSpace(x.NegativeBalanceUsers))
                .Select(x => x.NegativeBalanceUsers);

            return string.Join(",", validNegativeBalance);
        }
    }
}