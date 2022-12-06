using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.SignalR;
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
        private readonly TicketVoidingSystemHttpClientService ticketVoidingSystemHttpClientService;
        private readonly IHubContext<NotificationHub, ClientEvent> hubContext;
        private readonly LoggingService loggingService;

        public InsertPreVoidingDataStageImp(RequestDetailService requestDetailService,
            TicketVoidingQueueService ticketVoidingQueueService, TicketVoidingSystemHttpClientService ticketVoidingSystemHttpClientService, IHubContext<NotificationHub,
                ClientEvent> hubContext, LoggingService loggingService)
        {
            this.requestDetailService = requestDetailService;
            this.ticketVoidingQueueService = ticketVoidingQueueService;
            this.ticketVoidingSystemHttpClientService = ticketVoidingSystemHttpClientService;
            this.hubContext = hubContext;
            this.loggingService = loggingService;
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

            await NotifyRequestRunningToAllUser(queue.RequestId);
            await ticketVoidingQueueService.InsertQueue(siteDbGroupId, queue.RequestId, voidingHistory.VoidingHistoryId,
                validVoidSingleTickets, validVoidParlayTickets);
            logData.Add("InsertQueue-OK");

            return string.Empty;
        }

        private async Task NotifyRequestRunningToAllUser(int requestId)
        {
            try
            {
                var runningRequests = new List<RequestStatus>() {new RequestStatus
                    { RequestId = requestId, Status = RequestStatusConstant.Running }};
                await ticketVoidingSystemHttpClientService.SignalRUpdateRequestStatusToAllUsers(runningRequests);
                await hubContext.Clients.All.updateRequestStatus(runningRequests);
            }
            catch (Exception e)
            {
                await loggingService.ErrorAsync(e.Message);
            }
        }
    }
}