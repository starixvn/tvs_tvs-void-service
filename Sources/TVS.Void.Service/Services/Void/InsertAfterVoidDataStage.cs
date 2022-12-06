using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void
{
    public interface InsertAfterVoidDataStage
    {
        Task Execute(IEnumerable<TicketVoidingQueueModel> voidQueueResults);
    }

    public class InsertAfterVoidDataStageImp : InsertAfterVoidDataStage
    {
        private readonly RequestDetailService requestDetailService;

        public InsertAfterVoidDataStageImp(RequestDetailService requestDetailService)
        {
            this.requestDetailService = requestDetailService;
        }

        public async Task Execute(IEnumerable<TicketVoidingQueueModel> voidQueueResults)
        {
            var afterVoidData = voidQueueResults.Select(x => new AfterVoidServiceModel
            { RequestId = x.RequestId, VoidingHistoryId = x.VoidingHistoryId, MatchId = x.MatchId }).Distinct(new AfterVoidServiceModelEqualComparer());

            // get matchId of request

            var finalAfterVoidData = await AssignNegativeBalanceUsers(afterVoidData, voidServiceList);
            // TODO: in future if new issueType has Action Auto Void Winning, can't group by MatchId, must include condition IssueType
            var groupHaveMatchId = finalAfterVoidData.Where(x => x.MatchId > 0);
            var groupNoMatchId = finalAfterVoidData.Where(x => x.MatchId == 0);
            return (groupHaveMatchId, groupNoMatchId);
        }

        private async Task<IEnumerable<AfterVoidServiceModel>> AssignNegativeBalanceUsers(IEnumerable<AfterVoidServiceModel> afterVoidData, IEnumerable<VoidServiceModel> voidServiceList)
        {
            var finalData = afterVoidData.ToList();
            foreach (var item in finalData)
            {
                var affectedSiteDbList = voidServiceList.Where(x =>
                    x.RequestId == item.RequestId && x.VoidingHistoryId == item.VoidingHistoryId).Select(x => x.SiteDbGroupId).Distinct();

                item.NegativeBalanceUsers = await CombineNegativeBalanceUsers(item.VoidingHistoryId, affectedSiteDbList);
            }

            return finalData;
        }

        private async Task<int> GetMatchIdForAfterVoid(int issueTypeId, IEnumerable<TicketVoidingQueueModel> voidQueueResults)
        {
            var firstVoidQueue = voidQueueResults.First();
            var (_, requestDetails) = await requestDetailService.GetRequestById(firstVoidQueue.RequestId);

            const int parlayMatchId = 29;
            if (issueTypeId == IssueTypeConstant.AbnormalAccount || distinctMatchIdInRequest.Count() > 1 || distinctMatchIdInRequest.Any(x => x == parlayMatchId))
            {
                return 0;
            }

            return distinctMatchIdInRequest.First();
        }

        private string CombineNegativeBalanceUsers(IEnumerable<TicketVoidingQueueModel> voidQueueResults)
        {
            var validNegativeBalance = voidQueueResults.Where(x => !string.IsNullOrWhiteSpace(x.NegativeBalanceUsers))
                .Select(x => x.NegativeBalanceUsers);

            return string.Join(",", validNegativeBalance);
        }
    }
}