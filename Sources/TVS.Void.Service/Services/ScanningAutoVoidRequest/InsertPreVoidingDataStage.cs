using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface InsertPreVoidingDataStage
    {
        Task<(bool IsContinueFlow, string LogData)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets,
            IEnumerable<RequestDetailTicketModel> validVoidParlayTickets);
    }

    public class InsertPreVoidingDataStageImp : InsertPreVoidingDataStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly RequestDetailService requestDetailService;
        private readonly TicketVoidingQueueService ticketVoidingQueueService;

        public InsertPreVoidingDataStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService,
            TicketVoidingQueueService ticketVoidingQueueService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.requestDetailService = requestDetailService;
            this.ticketVoidingQueueService = ticketVoidingQueueService;
        }

        public async Task<(bool, string)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets)
        {
            if (validVoidSingleTickets == null && validVoidParlayTickets == null)
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "No ticket found.", null);

                return (false, null);
            }

            var validVoidRequests = GetDistinctValidVoidRequest(validVoidSingleTickets, validVoidParlayTickets);
            var voidServiceList = await requestDetailService.InsertVoidServiceData(queue.BroadcastSessionId, siteDbGroupId, validVoidRequests);
            var logData = $"{nameof(InsertPreVoidingDataStage)}: {JsonSerializer.Serialize(new { validVoidRequests, voidServiceList })}";

            await InsertTicketVoidingQueue(siteDbGroupId, voidServiceList, validVoidSingleTickets, validVoidParlayTickets);
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, default, null);
            return (true, logData);
        }

        private static IEnumerable<RequestDetailModel>
            GetDistinctValidVoidRequest(IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets)
        {
            var distinctRequestFirst = validVoidSingleTickets.Select(x => new RequestDetailModel
            { RequestId = x.RequestId, IssueTypeId = x.IssueTypeId }).Distinct();
            var distinctRequestSecond = validVoidParlayTickets.Select(x => new RequestDetailModel
            { RequestId = x.RequestId, IssueTypeId = x.IssueTypeId }).Distinct();
            var validVoidRequests = distinctRequestFirst.Union(distinctRequestSecond);

            return validVoidRequests;
        }

        private async Task InsertTicketVoidingQueue(int siteDbGroupId, IEnumerable<VoidServiceModel> voidServiceList,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets)
        {
            foreach (var voidService in voidServiceList)
            {
                var singleTicketsInCurrentRequest = validVoidSingleTickets.Where(x => x.RequestId == voidService.RequestId);
                var parlayTicketsInCurrentRequest = validVoidParlayTickets.Where(x => x.RequestId == voidService.RequestId);
                await ticketVoidingQueueService.InsertQueue(siteDbGroupId, voidService.VoidingHistoryId, voidService.RequestId, voidService.IssueTypeId,
                    singleTicketsInCurrentRequest, parlayTicketsInCurrentRequest);
            }
        }
    }
}