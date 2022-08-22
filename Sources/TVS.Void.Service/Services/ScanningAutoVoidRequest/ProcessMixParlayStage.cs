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
            if (queue.StatusId == ExecuteStatus.QueueMixParlay && !mixParlayRequests.Any())
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, null, true, null);

                return false;
            }

            var guidList = await InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(queue.StatusId, mixParlayRequests);
            logData.AppendLine($"Guid InsertAutoVoidQueue100: {guidList}");
            return true;
        }

        private async Task<string> InsertNewAutoVoidRequestQueueForMixParlaySpecialCase(int queueStatusId, IEnumerable<RequestModel> mixParlayRequests)
        {
            if (queueStatusId != ExecuteStatus.QueueMixParlay && mixParlayRequests.Any())
            {
                var logGuid = new StringBuilder();
                foreach (var request in mixParlayRequests)
                {
                    var guidInAllServer = Guid.NewGuid().ToString();
                    foreach (var server in SiteDbGroupConstant.ServerList)
                    {
                        await autoVoidRequestQueueService.InsertQueue(server.Id, guidInAllServer, request.RequestId, 0, ExecuteStatus.QueueMixParlay, AutoVoidConstant.QueueActionMixParlay);
                    }

                    logGuid.Append($",{guidInAllServer}");
                }

                return logGuid.ToString();
            }

            return string.Empty;
        }
    }
}