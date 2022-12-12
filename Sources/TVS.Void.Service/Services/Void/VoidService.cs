using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void
{
    public interface VoidService
    {
        Task Execute(int siteDbGroupId);
    }

    public class VoidServiceImp : VoidService
    {
        private readonly TicketVoidingQueueService ticketVoidingQueueService;
        private readonly RequestDetailService requestDetailService;
        private readonly ProcessVoidQueuesStage processVoidQueuesStage;
        private readonly InsertAfterVoidDataStage insertAfterVoidDataStage;
        private readonly NotificationService notificationService;
        private readonly LoggingService loggingService;

        public VoidServiceImp(TicketVoidingQueueService ticketVoidingQueueService, RequestDetailService requestDetailService, ProcessVoidQueuesStage processVoidQueuesStage,
            InsertAfterVoidDataStage insertAfterVoidDataStage, NotificationService notificationService, LoggingService loggingService)
        {
            this.ticketVoidingQueueService = ticketVoidingQueueService;
            this.requestDetailService = requestDetailService;
            this.processVoidQueuesStage = processVoidQueuesStage;
            this.insertAfterVoidDataStage = insertAfterVoidDataStage;
            this.notificationService = notificationService;
            this.loggingService = loggingService;
        }

        public async Task Execute(int siteDbGroupId)
        {
            var voidingQueues = await ticketVoidingQueueService.GetQueues(siteDbGroupId, null);
            if (!voidingQueues.Any())
            {
                return;
            }

            await ExecuteWhenHaveQueues(siteDbGroupId, voidingQueues);
        }

        private async Task ExecuteWhenHaveQueues(int siteDbGroupId, IEnumerable<TicketVoidingQueueModel> voidingQueues)
        {
            var logData = new List<string>();
            try
            {
                await ExecuteNormalFlow(siteDbGroupId, voidingQueues, logData);
            }
            catch (Exception e)
            {
                await loggingService.InfoJsonAsync(logData);
                await loggingService.ErrorAsync($"{nameof(VoidService)} Error", e);
            }
        }

        private async Task ExecuteNormalFlow(int siteDbGroupId, IEnumerable<TicketVoidingQueueModel> voidingQueues, IList<string> logData)
        {
            logData.Add($"{nameof(VoidService)}[{siteDbGroupId}]: {JsonSerializer.Serialize(voidingQueues)}");
            var processVoidQueuesResult = await processVoidQueuesStage.Execute(siteDbGroupId, voidingQueues);
            logData.Add($"{nameof(processVoidQueuesResult)}: {JsonSerializer.Serialize(processVoidQueuesResult)}");

            if (processVoidQueuesResult.Any(x => x.StatusId is ExecuteStatus.Failed or ExecuteStatus.New))
            {
                var firstVoidingQueue = voidingQueues.First();
                await requestDetailService.UpdateTicketVoidingHistory(firstVoidingQueue.VoidingHistoryId, ExecuteStatus.Failed);
                await notificationService.NotifyVoidFailedToUser(firstVoidingQueue.RequestId, processVoidQueuesResult);
                await notificationService.NotifyRequestFailedToAllUser(firstVoidingQueue.RequestId);
                await loggingService.InfoJsonAsync(logData);
                return;
            }

            if (processVoidQueuesResult.Any(x => x.StatusId == ExecuteStatus.Running))
            {
                await loggingService.InfoJsonAsync(logData);
                return;
            }

            await insertAfterVoidDataStage.Execute(processVoidQueuesResult, logData);

            logData.Add($"ServerTime: {DateTime.Now:MM/dd/yyyy HH:mm:ss.fff}");
            await loggingService.InfoJsonAsync(logData);
        }
    }
}