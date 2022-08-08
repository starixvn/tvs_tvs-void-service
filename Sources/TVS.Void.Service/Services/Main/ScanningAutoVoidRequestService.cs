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
        private StringBuilder logData;
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly RequestDetailService requestDetailService;
        private readonly TicketVoidingQueueService ticketVoidingQueueService;
        private readonly LoggingService loggingService;

        public ScanningAutoVoidRequestServiceImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService,
            TicketVoidingQueueService ticketVoidingQueueService, LoggingService loggingService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.requestDetailService = requestDetailService;
            this.ticketVoidingQueueService = ticketVoidingQueueService;
            this.loggingService = loggingService;
        }

        public async Task Execute(int siteDbGroupId)
        {
            logData = new StringBuilder();
            Stopwatch timer = new();
            timer.Start();
            var queue = await autoVoidRequestQueueService.GetValidQueue(siteDbGroupId);
            if (queue == null)
            {
                return;
            }

            try
            {
                logData.AppendLine($"Scanning[{siteDbGroupId}]: {JsonSerializer.Serialize(queue)}");
                if (queue.Action.Equals(AutoVoidConstant.QueueActionVoidInstant, StringComparison.OrdinalIgnoreCase)
                    || queue.Action.Equals(AutoVoidConstant.QueueActionVoidInstant5m, StringComparison.OrdinalIgnoreCase)
                    || queue.Action.Equals(AutoVoidConstant.QueueActionVoidInstant10m, StringComparison.OrdinalIgnoreCase)
                    || queue.Action.Equals(AutoVoidConstant.QueueActionVoidInstantNoPolicy, StringComparison.OrdinalIgnoreCase))
                {
                    await ExecuteForVoidInstantGroup(siteDbGroupId, queue);
                }
                else
                {
                    await ExecuteForAutoVoidWinningGroup(siteDbGroupId, queue);
                }

                timer.Stop();
                logData.AppendLine($"Server Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss}");
                logData.AppendLine($"Elapsed Time: {timer.ElapsedMilliseconds} ms");
                await loggingService.InfoAsync(logData.ToString());
            }
            catch (Exception e)
            {
                await loggingService.InfoAsync(logData.ToString());
                await loggingService.ErrorAsync("ScanningAutoVoidRequestService Error", e);
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Failed, null, null);
            }
        }

        public async Task ExecuteForVoidInstantGroup(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
            var (singleTickets, parlayTickets) = await requestDetailService.GetTicketsBySiteDbGroupId(siteDbGroupId, queue);
            if (singleTickets == null && parlayTickets == null)
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "No ticket found.", null);
                return;
            }

            var validVoidRequests = GetDistinctValidVoidRequest(singleTickets, parlayTickets);
            var voidServiceList = await requestDetailService.InsertVoidServiceData(queue.BroadcastSessionId, siteDbGroupId, validVoidRequests);

            await InsertTicketVoidingQueue(siteDbGroupId, voidServiceList, singleTickets, parlayTickets);

            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, default, null);
        }

        public async Task ExecuteForAutoVoidWinningGroup(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
            var (singleTickets, parlayTickets) = await requestDetailService.GetTicketsBySiteDbGroupId(siteDbGroupId, queue);
            if (singleTickets == null && parlayTickets == null)
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "No ticket found.", null);
                return;
            }

            var (validVoidSingleTickets, validVoidParlayTickets,
                    singleTicketsResettle, parlayTicketsResettle, mixParlayRequestsNoStatusWinLossButAllChildMatchesCompleted) =
                await requestDetailService.CheckStatusTickets(siteDbGroupId, singleTickets, parlayTickets);
            AddLogInfo($"{nameof(RequestDetailService.CheckStatusTickets)}", new object[] { validVoidSingleTickets, validVoidParlayTickets, singleTicketsResettle, parlayTicketsResettle,
                mixParlayRequestsNoStatusWinLossButAllChildMatchesCompleted });

            if (queue.StatusId == ExecuteStatus.QueueMixParlay && mixParlayRequestsNoStatusWinLossButAllChildMatchesCompleted == null)
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, null, null, 10);
                return;
            }

            await InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(queue.StatusId, mixParlayRequestsNoStatusWinLossButAllChildMatchesCompleted);

            // TODO: Check if this step fail -> re execute
            if (singleTicketsResettle != null || parlayTicketsResettle != null)
            {
                AddLogInfo($"{nameof(RequestDetailService.ReCalculateTickets)}", new object[] { singleTicketsResettle, parlayTicketsResettle });
                await requestDetailService.ReCalculateTickets(siteDbGroupId, singleTicketsResettle, parlayTicketsResettle);
            }

            if (validVoidSingleTickets == null && validVoidParlayTickets == null)
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "No ticket found.", null);
                return;
            }

            var validVoidRequests = GetDistinctValidVoidRequest(validVoidSingleTickets, validVoidParlayTickets);
            var voidServiceList = await requestDetailService.InsertVoidServiceData(queue.BroadcastSessionId, siteDbGroupId, validVoidRequests);
            AddLogInfo($"{nameof(RequestDetailService.InsertVoidServiceData)}", new object[] { validVoidRequests, voidServiceList });

            await InsertTicketVoidingQueue(siteDbGroupId, voidServiceList, validVoidSingleTickets, validVoidParlayTickets);

            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, default, null);
        }

        private async Task InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(int queueStatusId, IEnumerable<RequestDetailModel> mixParlayRequests)
        {
            if (queueStatusId != ExecuteStatus.QueueMixParlay && mixParlayRequests != null)
            {
                var guid = Guid.NewGuid().ToString();
                AddLogInfo("GUID InsertQueue", new object[] { guid });
                foreach (var server in SiteDbGroupConstant.ServerList)
                {
                    foreach (var request in mixParlayRequests)
                    {
                        await autoVoidRequestQueueService.InsertQueue(server.Id, guid, request.RequestId, 0, ExecuteStatus.QueueMixParlay, "MixParlay");
                    }
                }
            }
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

        private void AddLogInfo(string key, object[] data)
        {
            logData.AppendLine($"{key}: {JsonSerializer.Serialize(data)}");
        }
    }
}