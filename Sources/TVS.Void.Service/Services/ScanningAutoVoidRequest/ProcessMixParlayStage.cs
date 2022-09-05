using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface ProcessMixParlayStage
    {
        Task Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestModel> mixParlayRequests, string queueErrorMessage, StringBuilder logData);
    }

    public class ProcessMixParlayStageImp : ProcessMixParlayStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;

        public ProcessMixParlayStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        }

        public async Task Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestModel> mixParlayRequests, string queueErrorMessage, StringBuilder logData)
        {
            var isMixParlayQueue = queue.Action.Equals(AutoVoidConstant.QueueActionMixParlay, StringComparison.OrdinalIgnoreCase);
            if (!isMixParlayQueue)
            {
                await ExecuteForOthersCalcQueue(siteDbGroupId, queue, mixParlayRequests, queueErrorMessage, logData);
                return;
            }

            await ExecuteForMixParlayQueueOnly(siteDbGroupId, queue, mixParlayRequests, queueErrorMessage);
        }

        private async Task ExecuteForOthersCalcQueue(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestModel> mixParlayRequests, string queueErrorMessage, StringBuilder logData)
        {
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, null, ExecuteStatus.Done, null, queueErrorMessage);

            if (mixParlayRequests.Any())
            {
                await InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(mixParlayRequests, logData);
            }
        }

        private async Task InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(IEnumerable<RequestModel> mixParlayRequests, StringBuilder logData)
        {
            var guidList = new StringBuilder();
            foreach (var request in mixParlayRequests)
            {
                var guidInAllServer = $"{Guid.NewGuid()}_mp";
                foreach (var server in SiteDbGroupConstant.ServerList)
                {
                    await autoVoidRequestQueueService.InsertQueue(server.Id, guidInAllServer, request.RequestId, 0, ExecuteStatus.New, AutoVoidConstant.QueueActionMixParlay);
                }

                guidList.Append($",{guidInAllServer}");
            }

            logData.AppendLine($"Guid InsertMixParlayQueue: {guidList}");
        }

        private async Task ExecuteForMixParlayQueueOnly(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestModel> mixParlayRequests, string queueErrorMessage)
        {
            var currentRequest = mixParlayRequests.First(x => x.RequestId == queue.RequestId);
            switch (currentRequest.ProcessMixParlayStatus)
            {
                case ProcessMixParlayStatusConstant.PartialProcessTickets_WaitingForNextRunning:
                case ProcessMixParlayStatusConstant.NoTickets_WaitingForNextRunning:
                    await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, null, ExecuteStatus.New, true, null);
                    return;

                case ProcessMixParlayStatusConstant.DoneQueue:
                    await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, null, ExecuteStatus.Done, null, queueErrorMessage);
                    return;

                default:
                    return;
            }
        }
    }
}