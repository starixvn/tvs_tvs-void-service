using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest
{
    public interface InsertPreVoidingDataStage
    {
        Task<string> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets,
            IEnumerable<RequestDetailTicketModel> validVoidParlayTickets, IList<string> logData);
    }

    public class InsertPreVoidingDataStageImp : InsertPreVoidingDataStage
    {
        private readonly RequestDetailService requestDetailService;
        private readonly TicketVoidingQueueService ticketVoidingQueueService;
        private readonly RequestTicketService requestTicketService;
        private readonly NotificationService notificationService;

        public InsertPreVoidingDataStageImp(RequestDetailService requestDetailService,
            TicketVoidingQueueService ticketVoidingQueueService, RequestTicketService requestTicketService, NotificationService notificationService)
        {
            this.requestDetailService = requestDetailService;
            this.ticketVoidingQueueService = ticketVoidingQueueService;
            this.requestTicketService = requestTicketService;
            this.notificationService = notificationService;
        }

        public async Task<string> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets, IList<string> logData)
        {
            if (!validVoidSingleTickets.Any() && !validVoidParlayTickets.Any())
            {
                return "No ticket found.";
            }

            var requestIds = string.Join(",", DistinctRequestIdList(validVoidSingleTickets, validVoidParlayTickets));
            var voidingHistoryList = await requestDetailService.InsertVoidingHistory(requestIds, siteDbGroupId, queue.BroadcastSessionId);
            logData.Add($"{nameof(voidingHistoryList)}: {JsonSerializer.Serialize(voidingHistoryList)}");

            await notificationService.NotifyRequestRunningToAllUser(voidingHistoryList);
            await ticketVoidingQueueService.InsertQueueForRequests(siteDbGroupId, voidingHistoryList,
                validVoidSingleTickets, validVoidParlayTickets);
            logData.Add("InsertQueues-OK");

            await requestTicketService.UpdateIsProcessed(validVoidSingleTickets, validVoidParlayTickets);
            logData.Add($"{nameof(RequestTicketService.UpdateIsProcessed)}-AfterInsertVoidQueue-OK");

            return string.Empty;
        }

        private static IEnumerable<int> DistinctRequestIdList(IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets)
        {
            var requestIdInSingleTickets = validVoidSingleTickets.Select(x => x.RequestId).Distinct();
            var requestIdInParlayTickets = validVoidParlayTickets.Select(x => x.RequestId).Distinct();

            return requestIdInSingleTickets.Union(requestIdInParlayTickets);
        }
    }
}