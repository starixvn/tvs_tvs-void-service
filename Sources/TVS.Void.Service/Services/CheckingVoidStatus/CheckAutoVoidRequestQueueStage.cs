using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckingVoidStatus
{
    public interface CheckAutoVoidRequestQueueStage
    {
        Task<bool> Execute(bool isAutoVoid, string broadcastSessionId, int createdTimeDuration, StringBuilder logData);
    }

    public class CheckAutoVoidRequestQueueStageImp : CheckAutoVoidRequestQueueStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly RequestDetailService requestDetailService;
        private const int FiveMinutesInSecond = 300;

        public CheckAutoVoidRequestQueueStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.requestDetailService = requestDetailService;
        }

        public async Task<bool> Execute(bool isAutoVoid, string broadcastSessionId, int createdTimeDuration, StringBuilder logData)
        {
            if (isAutoVoid)
            {
                var queuesInAllServer = await GetAutoVoidRequestQueueInAllServerDb(broadcastSessionId);
                logData.AppendLine($"AutoVoidRequestQueues: {JsonSerializer.Serialize(queuesInAllServer)}");

                var isMixParlayQueue = broadcastSessionId.Contains("_mp");
                if (!isMixParlayQueue)
                {
                    return await ExecuteForOthersCalcQueue(broadcastSessionId, createdTimeDuration, queuesInAllServer, logData);
                }

                return await ExecuteForMixParlayQueueOnly(broadcastSessionId, createdTimeDuration, queuesInAllServer, logData);
            }

            return true;
        }

        public async Task<bool> ExecuteForOthersCalcQueue(string broadcastSessionId, int createdTimeDuration, IEnumerable<AutoVoidRequestQueueModel> queuesInAllServer, StringBuilder logData)
        {
            if (createdTimeDuration > FiveMinutesInSecond)
            {
                await requestDetailService.UpdateVoidServiceByBroadcastSessionId(broadcastSessionId, null, true);
                logData.AppendLine("UpdatedCreatedDateVoidServiceByBroadcastSessionId");
            }

            if (queuesInAllServer.Any(x => x == null)
            || queuesInAllServer.Any(x => x.StatusId == ExecuteStatus.Failed)
            || queuesInAllServer.Any(x => x.StatusId != ExecuteStatus.Done))
            {
                return false;
            }

            return true;
        }

        public async Task<bool> ExecuteForMixParlayQueueOnly(string broadcastSessionId, int createdTimeDuration, IEnumerable<AutoVoidRequestQueueModel> queuesInAllServer, StringBuilder logData)
        {
            //// case re-run check service but current session has been changed => auto pass
            if (queuesInAllServer.All(x => x == null))
            {
                return true;
            }

            if (queuesInAllServer.Any(x => x.StatusId == ExecuteStatus.Failed) || createdTimeDuration > FiveMinutesInSecond)
            {
                await requestDetailService.UpdateVoidServiceByBroadcastSessionId(broadcastSessionId, null, true);
                logData.AppendLine("UpdatedCreatedDateVoidServiceByBroadcastSessionId");
                return false;
            }

            if (queuesInAllServer.Any(x => x.StatusId == ExecuteStatus.New))
            {
                var newBroadcastSessionId = GetNextExecutionMixParlayBroadcastSessionId(broadcastSessionId);
                foreach (var queue in queuesInAllServer)
                {
                    await autoVoidRequestQueueService.UpdateQueue(queue.SiteDbGroupId, queue.QueueId, newBroadcastSessionId, null, null, null);
                }

                logData.AppendLine($"UpdatedAutoVoidRequestQueue-NewBroadcastSessionId: {newBroadcastSessionId}");
                return true;
            }

            return true;
        }

        private static string GetNextExecutionMixParlayBroadcastSessionId(string broadcastSessionId)
        {
            var indexOfMixParlay = broadcastSessionId.IndexOf("_mp", StringComparison.OrdinalIgnoreCase);
            var currentGuid = broadcastSessionId[..indexOfMixParlay];
            var numberAfterMpText = broadcastSessionId[(indexOfMixParlay + 3)..];
            int.TryParse(numberAfterMpText, out var currentExecutionTimes);

            return $"{currentGuid}_mp{currentExecutionTimes + 1}";
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