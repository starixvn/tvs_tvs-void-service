using System.Collections.Generic;
using System.Linq;
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
        private readonly NotificationService notificationService;

        public InsertPreVoidingDataStageImp(RequestDetailService requestDetailService,
            TicketVoidingQueueService ticketVoidingQueueService, NotificationService notificationService)
        {
            this.requestDetailService = requestDetailService;
            this.ticketVoidingQueueService = ticketVoidingQueueService;
            this.notificationService = notificationService;
        }

        public async Task<string> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets, IList<string> logData)
        {
            if (!validVoidSingleTickets.Any() && !validVoidParlayTickets.Any())
            {
                return "No ticket found.";
            }

            var voidingHistory = await requestDetailService.InsertVoidingHistory(queue.RequestId);
            logData.Add($"{nameof(voidingHistory)}: {voidingHistory.VoidingHistoryId}");

            await notificationService.NotifyRequestRunningToAllUser(queue.RequestId);
            await ticketVoidingQueueService.InsertQueue(siteDbGroupId, queue.RequestId, voidingHistory.VoidingHistoryId,
                validVoidSingleTickets, validVoidParlayTickets);
            logData.Add("InsertQueue-OK");

            return string.Empty;
        }
    }
}