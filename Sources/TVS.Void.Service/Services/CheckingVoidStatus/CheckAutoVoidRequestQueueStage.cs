using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckingVoidStatus
{
    public interface CheckAutoVoidRequestQueueStage
    {
        Task<(bool IsContinueFlow, string LogData)> Execute(bool isAutoVoid, string broadcastSessionId, int createdTimeDuration);
    }

    public class CheckAutoVoidRequestQueueStageImp : CheckAutoVoidRequestQueueStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly RequestDetailService requestDetailService;

        public CheckAutoVoidRequestQueueStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.requestDetailService = requestDetailService;
        }

        public async Task<(bool IsContinueFlow, string LogData)> Execute(bool isAutoVoid, string broadcastSessionId, int createdTimeDuration)
        {
            var logData = string.Empty;
            if (isAutoVoid)
            {
                var autoVoidRequestQueues = await GetAutoVoidRequestQueueInAllServerDb(broadcastSessionId);
                logData = $"AutoVoidRequestQueues: {JsonSerializer.Serialize(autoVoidRequestQueues)}";

                if (autoVoidRequestQueues.Any(x => x == null))

                {
                    return (false, logData);
                }

                if (autoVoidRequestQueues.Any(x => x.StatusId == ExecuteStatus.Failed) || createdTimeDuration > 300)
                {
                    await requestDetailService.UpdateVoidServiceByBroadcastSessionId(broadcastSessionId, null, true);
                    return (false, logData);
                }

                if (autoVoidRequestQueues.Any(x => x.StatusId == ExecuteStatus.New))
                {
                    return (false, logData);
                }
            }

            return (true, logData);
        }

        private async Task<IEnumerable<AutoVoidRequestQueueModel>> GetAutoVoidRequestQueueInAllServerDb(string broadcastSessionId)
        {
            var autoVoidRequestQueues = new List<AutoVoidRequestQueueModel>();
            foreach (var server in SiteDbGroupConstant.ServerList)
            {
                var queue = await autoVoidRequestQueueService.GetQueue(server.Id, broadcastSessionId);
                autoVoidRequestQueues.Add(queue);
            }

            return autoVoidRequestQueues;
        }
    }
}