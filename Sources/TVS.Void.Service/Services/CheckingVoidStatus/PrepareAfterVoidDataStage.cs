using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckingVoidStatus
{
    public interface PrepareAfterVoidDataStage
    {
        Task<(IEnumerable<AfterVoidServiceModel> GroupHaveMatchId, IEnumerable<AfterVoidServiceModel> GroupNoMatchId)>
            Execute(IEnumerable<VoidServiceModel> voidServiceList);
    }

    public class PrepareAfterVoidDataStageImp : PrepareAfterVoidDataStage
    {
        private readonly TicketVoidingQueueService ticketVoidingQueueService;

        public PrepareAfterVoidDataStageImp(TicketVoidingQueueService ticketVoidingQueueService)
        {
            this.ticketVoidingQueueService = ticketVoidingQueueService;
        }

        public async Task<(IEnumerable<AfterVoidServiceModel> GroupHaveMatchId, IEnumerable<AfterVoidServiceModel> GroupNoMatchId)>
            Execute(IEnumerable<VoidServiceModel> voidServiceList)
        {
            var afterVoidData = voidServiceList.Select(x => new AfterVoidServiceModel
            { RequestId = x.RequestId, VoidingHistoryId = x.VoidingHistoryId, MatchId = x.MatchId }).Distinct(new AfterVoidServiceModelEqualComparer());

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

                item.NegativeBalanceUsers = await GetNegativeBalanceUsersInAllVoidingQueues(item.VoidingHistoryId, affectedSiteDbList);
            }

            return finalData;
        }

        private async Task<string> GetNegativeBalanceUsersInAllVoidingQueues(int voidingHistoryId, IEnumerable<int> affectedSiteDbList)
        {
            var negativeBalanceUsers = new List<string>();
            foreach (var siteId in affectedSiteDbList)
            {
                var voidingQueues = await ticketVoidingQueueService.GetQueues(siteId, voidingHistoryId);
                var validNegativeBalance = voidingQueues.Where(x => !string.IsNullOrWhiteSpace(x.NegativeBalanceUsers))
                    .Select(x => x.NegativeBalanceUsers);
                negativeBalanceUsers.AddRange(validNegativeBalance);
            }

            return string.Join(",", negativeBalanceUsers);
        }
    }
}