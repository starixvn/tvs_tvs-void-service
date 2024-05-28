using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckTicketStatus;

public interface InsertAutoVoidQueueStage
{
    Task ExecuteForAutoVoidWinningAndResultKeyed(int siteDbGroupId, IEnumerable<RequestModel> requests, string actionName,
        IDictionary<int, string> broadcastSessionIdCacheByMatch, IList<string> logData);
}

public class InsertAutoVoidQueueStageImp : InsertAutoVoidQueueStage
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;

    public InsertAutoVoidQueueStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService)
    {
        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
    }

    public async Task ExecuteForAutoVoidWinningAndResultKeyed(int siteDbGroupId, IEnumerable<RequestModel> requests, string actionName,
        IDictionary<int, string> broadcastSessionIdCacheByMatch, IList<string> logData)
    {
        if (!requests.Any())
        {
            return;
        }

        var groupRequestByMatch = requests.GroupBy(x => x.MatchId);
        foreach (var group in groupRequestByMatch)
        {
            if (!broadcastSessionIdCacheByMatch.TryGetValue(group.Key, out var currentBroadcastSessionId))
            {
                currentBroadcastSessionId = Guid.NewGuid().ToString();
                broadcastSessionIdCacheByMatch.Add(group.Key, currentBroadcastSessionId);
            }

            var requestIds = string.Join(",", group.Select(x => x.RequestId));
            // DB must check exist queue Status = New => not insert
            await autoVoidRequestQueueService.InsertQueue(siteDbGroupId, currentBroadcastSessionId, requestIds, ExecuteStatus.New, actionName, group.Key);

            logData.Add($"{nameof(InsertAutoVoidQueueStage)}-{actionName}-[{siteDbGroupId}]: {requestIds} - {group.Key} - {currentBroadcastSessionId}");
        }
    }
}