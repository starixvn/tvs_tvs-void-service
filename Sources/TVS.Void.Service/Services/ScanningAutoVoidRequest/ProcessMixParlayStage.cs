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
        Task<bool> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestModel> mixParlayRequests, StringBuilder logData);
    }

    public class ProcessMixParlayStageImp : ProcessMixParlayStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;

        public ProcessMixParlayStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        }

        public async Task<bool> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestModel> mixParlayRequests, StringBuilder logData)
        {
            var isMixParlayQueue = queue.Action.Equals(AutoVoidConstant.QueueActionMixParlay, StringComparison.OrdinalIgnoreCase);
            if (isMixParlayQueue)
            {
                return await ExecuteForMixParlayQueueOnly(siteDbGroupId, queue, mixParlayRequests, logData);
            }

            return await ExecuteForOthersCalcQueue(mixParlayRequests, logData);
        }

        private async Task<bool> ExecuteForMixParlayQueueOnly(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestModel> mixParlayRequests, StringBuilder logData)
        {
            var currentRequest = mixParlayRequests.First(x => x.RequestId == queue.RequestId);
            switch (currentRequest.ProcessMixParlayStatus)
            {
                case ProcessMixParlayStatusConstant.InsertNewMixParlayQueue:
                    await InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(mixParlayRequests, logData);
                    return true;

                case ProcessMixParlayStatusConstant.DoneQueueWithNoTicketFound:
                    return true;

                case ProcessMixParlayStatusConstant.WaitingForNextRunning:
                    await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, null, true, null);
                    return false;

                default:
                    return false;
            }
        }

        private async Task<bool> ExecuteForOthersCalcQueue(IEnumerable<RequestModel> mixParlayRequests, StringBuilder logData)
        {
            if (mixParlayRequests.Any())
            {
                await InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(mixParlayRequests, logData);
            }

            return true;
        }

        private async Task InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(IEnumerable<RequestModel> mixParlayRequests, StringBuilder logData)
        {
            var guidList = new StringBuilder();
            foreach (var request in mixParlayRequests)
            {
                var guidInAllServer = Guid.NewGuid().ToString();
                foreach (var server in SiteDbGroupConstant.ServerList)
                {
                    await autoVoidRequestQueueService.InsertQueue(server.Id, guidInAllServer, request.RequestId, 0, ExecuteStatus.QueueMixParlay, AutoVoidConstant.QueueActionMixParlay);
                }

                guidList.Append($",{guidInAllServer}");
            }

            logData.AppendLine($"Guid InsertAutoVoidQueue100: {guidList}");
        }
    }
}