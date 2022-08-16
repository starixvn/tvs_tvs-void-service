using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void
{
    public interface VoidService
    {
        Task Execute(int siteDbGroupId);
    }

    public class VoidServiceImp : VoidService
    {
        private StringBuilder logData;
        private readonly Stopwatch timer = new();
        private readonly TicketVoidingQueueService ticketVoidingQueueService;
        private readonly RequestDetailService requestDetailService;
        private readonly ProcessVoidQueuesStage processVoidQueuesStage;
        private readonly TicketVoidingSystemHttpClientService ticketVoidingSystemHttpClientService;
        private readonly IHubContext<NotificationHub, ClientEvent> hubContext;
        private readonly LoggingService loggingService;

        public VoidServiceImp(TicketVoidingQueueService ticketVoidingQueueService, RequestDetailService requestDetailService, ProcessVoidQueuesStage processVoidQueuesStage,
            TicketVoidingSystemHttpClientService ticketVoidingSystemHttpClientService, IHubContext<NotificationHub, ClientEvent> hubContext, LoggingService loggingService)
        {
            this.ticketVoidingQueueService = ticketVoidingQueueService;
            this.requestDetailService = requestDetailService;
            this.processVoidQueuesStage = processVoidQueuesStage;
            this.ticketVoidingSystemHttpClientService = ticketVoidingSystemHttpClientService;
            this.hubContext = hubContext;
            this.loggingService = loggingService;
        }

        public async Task Execute(int siteDbGroupId)
        {
            logData = new StringBuilder();
            timer.Start();
            var voidingQueues = await ticketVoidingQueueService.GetQueues(siteDbGroupId, null);
            if (!voidingQueues.Any())
            {
                return;
            }

            await ExecuteWhenHaveData(siteDbGroupId, voidingQueues);
        }

        private async Task ExecuteWhenHaveData(int siteDbGroupId, IEnumerable<TicketVoidingQueueModel> voidingQueues)
        {
            try
            {
                logData.AppendLine($"{nameof(VoidService)}[{siteDbGroupId}]: {JsonSerializer.Serialize(voidingQueues)}");
                var processVoidQueuesResult = await processVoidQueuesStage.Execute(siteDbGroupId, voidingQueues);
                logData.AppendLine($"{nameof(ProcessVoidQueueResult)}: {JsonSerializer.Serialize(processVoidQueuesResult)}");

                var firstVoidingQueue = voidingQueues.First();
                if (processVoidQueuesResult.Any(x => x.StatusId == ExecuteStatus.Failed))
                {
                    await requestDetailService.UpdateVoidService(siteDbGroupId, firstVoidingQueue.RequestId, firstVoidingQueue.VoidingHistoryId, ExecuteStatus.Failed);
                    await requestDetailService.UpdateTicketVoidingHistory(firstVoidingQueue.VoidingHistoryId, ExecuteStatus.Failed);
                    await NotifyVoidFailedToUser(firstVoidingQueue.RequestId, processVoidQueuesResult);
                    await NotifyRequestFailedToAllUser(firstVoidingQueue.RequestId);
                    await loggingService.InfoAsync(logData.ToString());
                    return;
                }

                if (processVoidQueuesResult.Any(x => x.StatusId == ExecuteStatus.Running))
                {
                    await loggingService.InfoAsync(logData.ToString());
                    return;
                }
                await requestDetailService.UpdateVoidService(siteDbGroupId, firstVoidingQueue.RequestId, firstVoidingQueue.VoidingHistoryId, ExecuteStatus.VoidServiceExecuteVoidDone);

                timer.Stop();
                logData.AppendLine($"Server Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss.fff}");
                logData.AppendLine($"Elapsed Time: {timer.ElapsedMilliseconds} ms");
                await loggingService.InfoAsync(logData.ToString());
            }
            catch (Exception e)
            {
                await loggingService.InfoAsync(logData.ToString());
                await loggingService.ErrorAsync($"{nameof(VoidService)} Error", e);
            }
        }

        private async Task NotifyVoidFailedToUser(int requestId, IEnumerable<ProcessVoidQueueResult> voidQueueResults)
        {
            var voidResultHasUserInfo = voidQueueResults.First(x => x.UserId > 0);
            var errorMessage = $"Failed to void Request ID: {requestId}, please contact IT for support. (Error: VOID_SERVICE)";
            await ticketVoidingSystemHttpClientService.SignalRNotifyErrorMessageToUser(voidResultHasUserInfo.UserName, errorMessage);
            await hubContext.Clients.User(voidResultHasUserInfo.UserName).showErrorMessage(errorMessage);
        }

        private async Task NotifyRequestFailedToAllUser(int requestId)
        {
            var failedRequests = new List<RequestStatus>
                {new () {RequestId = requestId, Status = RequestStatusConstant.Failed}};
            await ticketVoidingSystemHttpClientService.SignalRUpdateRequestStatusToAllUsers(failedRequests);
            await hubContext.Clients.All.updateRequestStatus(failedRequests);
        }
    }
}