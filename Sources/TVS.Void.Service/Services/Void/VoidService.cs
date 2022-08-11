using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TVS.Void.Service.Constants;
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
        private readonly TicketVoidingQueueService ticketVoidingQueueService;
        private readonly RequestDetailService requestDetailService;
        private readonly LoggingService loggingService;

        public VoidServiceImp(TicketVoidingQueueService ticketVoidingQueueService, RequestDetailService requestDetailService, LoggingService loggingService)
        {
            this.ticketVoidingQueueService = ticketVoidingQueueService;
            this.requestDetailService = requestDetailService;
            this.loggingService = loggingService;
        }

        public async Task Execute(int siteDbGroupId)
        {
            logData = new StringBuilder();
            Stopwatch timer = new();
            timer.Start();
            var voidingQueues = await ticketVoidingQueueService.GetQueues(siteDbGroupId);
            if (voidingQueues == null)
            {
                return;
            }

            try
            {
                logData.AppendLine($"{nameof(VoidService)}[{siteDbGroupId}]: {JsonSerializer.Serialize(voidingQueues)}");
                var processVoidQueuesResult = await ProcessVoidQueues(siteDbGroupId, voidingQueues);

                var firstVoidingQueue = voidingQueues.First();
                if (processVoidQueuesResult.Any(x => x.StatusId == ExecuteStatus.Failed))
                {
                    await requestDetailService.UpdateVoidService(siteDbGroupId, firstVoidingQueue.RequestId, firstVoidingQueue.VoidingHistoryId, ExecuteStatus.Failed);
                    return;
                }

                if (processVoidQueuesResult.Any(x => x.StatusId == ExecuteStatus.Running))
                {
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

        private async Task<IEnumerable<ProcessVoidQueueResult>> ProcessVoidQueues(int siteDbGroupId, IEnumerable<TicketVoidingQueueModel> voidingQueues)
        {
            var processResult = new List<ProcessVoidQueueResult>();
            foreach (var queue in voidingQueues)
            {
                if (queue.StatusId is ExecuteStatus.Done or ExecuteStatus.Failed)
                {
                    processResult.Add(new ProcessVoidQueueResult { QueueId = queue.QueueId, StatusId = queue.StatusId });
                    continue;
                }

                var voidResult = await ticketVoidingQueueService.ProcessVoidQueue(siteDbGroupId, queue.QueueId);
                processResult.Add(voidResult);
            }

            logData.AppendLine($"{nameof(ProcessVoidQueueResult)}: {JsonSerializer.Serialize(processResult)}");

            return processResult;
        }
    }
}