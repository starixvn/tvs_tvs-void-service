using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Main
{
    public interface ScanningAutoVoidRequestService
    {
        Task Execute(int siteDbGroupId);
    }

    public class ScanningAutoVoidRequestServiceImp : ScanningAutoVoidRequestService
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly RequestDetailService requestDetailService;
        private readonly LoggingService loggingService;

        public ScanningAutoVoidRequestServiceImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService, LoggingService loggingService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.requestDetailService = requestDetailService;
            this.loggingService = loggingService;
        }

        public async Task Execute(int siteDbGroupId)
        {
            var timer = new Stopwatch();
            timer.Start();
            var queue = await autoVoidRequestQueueService.GetValidQueue(siteDbGroupId);
            if (queue == null)
            {
                return;
            }

            var (validSingleTickets, validParlayTickets, validRequests) =
                await requestDetailService.CheckTicketsAllowToVoid(siteDbGroupId, queue);

            if (validRequests == null || !validRequests.Any())
            {
                await autoVoidRequestQueueService.UpdateQueueStatus(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "No ticket found.");
                return;
            }

            var voidServiceList = await requestDetailService.InsertVoidServiceData(queue.BroadcastSessionId, siteDbGroupId, validRequests);
            foreach (var voidService in voidServiceList)
            {
                var singleTicketsInCurrentRequest = validSingleTickets.Where(x => x.RequestId == voidService.RequestId);
                var parlayTicketsInCurrentRequest = validParlayTickets.Where(x => x.RequestId == voidService.RequestId);
                await requestDetailService.InsertTicketVoidingQueue(voidService.VoidingHistoryId, queue.MatchId, voidService.RequestId, voidService.IssueTypeId,
                    singleTicketsInCurrentRequest, parlayTicketsInCurrentRequest);
            }

            await autoVoidRequestQueueService.UpdateQueueStatus(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, default);

            timer.Stop();
            await WriteLog(timer.ElapsedMilliseconds, siteDbGroupId, queue, voidServiceList);
        }

        private async Task WriteLog(long elapsedMilliseconds, int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<VoidServiceModel> voidServiceList)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"ScanningAutoVoidRequestService[{siteDbGroupId}] - {elapsedMilliseconds} ms - Action: {queue.Action} - MatchId: {queue.MatchId} - RequestId: {queue.RequestId}");
            stringBuilder.AppendLine($"Void_Service: {JsonSerializer.Serialize(voidServiceList)}");
            stringBuilder.AppendLine($"Server Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss}");
            await loggingService.InfoAsync(stringBuilder.ToString());
        }
    }
}