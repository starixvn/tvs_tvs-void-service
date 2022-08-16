using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckingVoidStatus
{
    public interface PrepareAfterVoidDataStage
    {
        Task<IEnumerable<AfterVoidServiceModel>> Execute(IEnumerable<VoidServiceModel> voidServiceList);
    }

    public class PrepareAfterVoidDataStageImp : PrepareAfterVoidDataStage
    {
        private readonly TicketVoidingQueueService ticketVoidingQueueService;

        public PrepareAfterVoidDataStageImp(TicketVoidingQueueService ticketVoidingQueueService)
        {
            this.ticketVoidingQueueService = ticketVoidingQueueService;
        }

        public async Task<IEnumerable<AfterVoidServiceModel>> Execute(IEnumerable<VoidServiceModel> voidServiceList)
        {
            var afterVoidData = voidServiceList.Select(x => new AfterVoidServiceModel
            { RequestId = x.RequestId, VoidingHistoryId = x.VoidingHistoryId, MatchId = x.MatchId }).Distinct(new AfterVoidServiceModelEqualComparer());

            await AssignNegativeBalanceUsers(afterVoidData, voidServiceList);

            return afterVoidData;
        }

        private async Task AssignNegativeBalanceUsers(IEnumerable<AfterVoidServiceModel> afterVoidData, IEnumerable<VoidServiceModel> voidServiceList)
        {
            foreach (var item in afterVoidData)
            {
                var affectedSiteDbList = voidServiceList.Where(x => x.RequestId == item.RequestId && x.VoidingHistoryId == item.VoidingHistoryId)
                    .Select(x => x.SiteDbGroupId).Distinct();

                item.NegativeBalanceUsers = await GetNegativeBalanceUsersInAllVoidingQueues(item.VoidingHistoryId, affectedSiteDbList);
            }
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