using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface ProcessMixParlayStage
    {
        Task<(bool IsContinueFlow, string LogData)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestModel> mixParlayRequests);
    }

    public class ProcessMixParlayStageImp : ProcessMixParlayStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;

        public ProcessMixParlayStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        }

        public async Task<(bool IsContinueFlow, string LogData)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestModel> mixParlayRequests)
        {
            if (queue.StatusId == ExecuteStatus.QueueMixParlay && !mixParlayRequests.Any())
            {
                const int minuteOffset = 10;
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, null, null, minuteOffset);

                return (false, null);
            }

            var guid = await InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(queue.StatusId, mixParlayRequests);
            var logData = $"Guid InsertAutoVoidQueue100: {guid}";
            return (true, logData);
        }

        private async Task<string> InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(int queueStatusId, IEnumerable<RequestModel> mixParlayRequests)
        {
            if (queueStatusId != ExecuteStatus.QueueMixParlay && mixParlayRequests.Any())
            {
                var guid = Guid.NewGuid().ToString();

                foreach (var server in SiteDbGroupConstant.ServerList)
                {
                    foreach (var request in mixParlayRequests)
                    {
                        await autoVoidRequestQueueService.InsertQueue(server.Id, guid, request.RequestId, 0, ExecuteStatus.QueueMixParlay, AutoVoidConstant.QueueActionMixParlay);
                    }
                }

                return guid;
            }

            return string.Empty;
        }
    }
}