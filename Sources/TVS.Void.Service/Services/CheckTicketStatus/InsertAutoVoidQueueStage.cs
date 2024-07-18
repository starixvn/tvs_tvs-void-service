using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckTicketStatus;

public interface InsertAutoVoidQueueStage
{
    Task ExecuteForAutoVoidWinning(int siteDbGroupId, IEnumerable<RequestModel> requests,
        IDictionary<int, string> broadcastSessionIdCacheByMatch, IList<string> logData);

    Task ExecuteForAutoVoidAllResultKeyed(int siteDbGroupId, IEnumerable<RequestModel> requests,
        IDictionary<int, string> broadcastSessionIdCacheByRequestId, IList<string> logData);
}

public class InsertAutoVoidQueueStageImp : InsertAutoVoidQueueStage
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;

    public InsertAutoVoidQueueStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService)
    {
        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
    }

    public async Task ExecuteForAutoVoidWinning(int siteDbGroupId, IEnumerable<RequestModel> requests,
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
            await autoVoidRequestQueueService.InsertQueue(siteDbGroupId, currentBroadcastSessionId, requestIds, ExecuteStatus.New, AutoVoidConstant.QueueActionCal, group.Key);

            logData.Add($"{nameof(InsertAutoVoidQueueStage)}-{AutoVoidConstant.QueueActionCal}-[{siteDbGroupId}]: {requestIds} - {group.Key} - {currentBroadcastSessionId}");
        }
    }

    public async Task ExecuteForAutoVoidAllResultKeyed(int siteDbGroupId, IEnumerable<RequestModel> requests,
        IDictionary<int, string> broadcastSessionIdCacheByRequestId, IList<string> logData)
    {
        foreach (var requestId in requests.Select(x => x.RequestId))
        {
            if (!broadcastSessionIdCacheByRequestId.TryGetValue(requestId, out var currentBroadcastSessionId))
            {
                currentBroadcastSessionId = Guid.NewGuid().ToString();
                broadcastSessionIdCacheByRequestId.Add(requestId, currentBroadcastSessionId);
            }

            // DB must check exist queue Status = New => not insert
            await autoVoidRequestQueueService.InsertQueue(siteDbGroupId, currentBroadcastSessionId, requestId.ToString(), ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);

            logData.Add($"{nameof(InsertAutoVoidQueueStage)}-{AutoVoidConstant.QueueActionVoidResultKeyed}-[{siteDbGroupId}]: {requestId} - 0 - {currentBroadcastSessionId}");
        }
    }
}