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
        Task<(bool IsContinueFlow, string LogData)> Execute(bool isAutoVoid, string broadcastSessionId);
    }

    public class CheckAutoVoidRequestQueueStageImp : CheckAutoVoidRequestQueueStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;

        public CheckAutoVoidRequestQueueStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        }

        public async Task<(bool IsContinueFlow, string LogData)> Execute(bool isAutoVoid, string broadcastSessionId)
        {
            var logData = string.Empty;
            if (isAutoVoid)
            {
                var autoVoidRequestQueues = new List<AutoVoidRequestQueueModel>();
                foreach (var server in SiteDbGroupConstant.ServerList)
                {
                    var queue = await autoVoidRequestQueueService.GetQueue(server.Id, broadcastSessionId);
                    autoVoidRequestQueues.Add(queue);
                }

                logData = $"autoVoidRequestQueues: {JsonSerializer.Serialize(autoVoidRequestQueues)}";
                if (autoVoidRequestQueues.Any(x => x == null)
                    || autoVoidRequestQueues.Any(x => x.StatusId != ExecuteStatus.Done))
                {
                    return (false, logData);
                }
            }

            return (true, logData);
        }
    }
}