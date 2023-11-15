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
    Task ExecuteForAutoVoidWinning(int siteDbGroupId, IEnumerable<RequestModel> requests, string broadcastSessionId, IList<string> logData);

    Task ExecuteForAutoVoidAllResultKeyed(int siteDbGroupId, IEnumerable<RequestModel> requests,
        IDictionary<int, string> broadcastSessionIdCache, IList<string> logData);
}

public class InsertAutoVoidQueueStageImp : InsertAutoVoidQueueStage
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;

    public InsertAutoVoidQueueStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService)
    {
        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
    }

    public async Task ExecuteForAutoVoidWinning(int siteDbGroupId, IEnumerable<RequestModel> requests, string broadcastSessionId, IList<string> logData)
    {
        if (requests.Any())
        {
            var requestIds = string.Join(",", requests.Select(x => x.RequestId));
            // DB must check exist queue Status = New => not insert
            await autoVoidRequestQueueService.InsertQueue(siteDbGroupId, broadcastSessionId, requestIds, ExecuteStatus.New, AutoVoidConstant.QueueActionCal, null);

            logData.Add($"{nameof(InsertAutoVoidQueueStage)}-{AutoVoidConstant.QueueActionCal}-[{siteDbGroupId}]: {requestIds} - {broadcastSessionId}");
        }
    }

    public async Task ExecuteForAutoVoidAllResultKeyed(int siteDbGroupId, IEnumerable<RequestModel> requests, IDictionary<int, string> broadcastSessionIdCache, IList<string> logData)
    {
        foreach (var requestId in requests.Select(x => x.RequestId))
        {
            if (!broadcastSessionIdCache.TryGetValue(requestId, out var currentBroadcastSessionId))
            {
                currentBroadcastSessionId = Guid.NewGuid().ToString();
                broadcastSessionIdCache.Add(requestId, currentBroadcastSessionId);
            }

            // DB must check exist queue Status = New => not insert
            await autoVoidRequestQueueService.InsertQueue(siteDbGroupId, currentBroadcastSessionId, requestId.ToString(),
                ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);

            logData.Add($"{nameof(InsertAutoVoidQueueStage)}-{AutoVoidConstant.QueueActionVoidResultKeyed}-[{siteDbGroupId}]: {requestId} - {currentBroadcastSessionId}");
        }
    }
}