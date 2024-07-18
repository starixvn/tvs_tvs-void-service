using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface CheckVoidWinningRequestQueueValidStage
{
    Task<(bool IsValid, string RequestIdsChangedAction)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue);
}

public class CheckVoidWinningRequestQueueValidStageImp : CheckVoidWinningRequestQueueValidStage
{
    private readonly RequestDetailService requestDetailService;
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;

    public CheckVoidWinningRequestQueueValidStageImp(RequestDetailService requestDetailService, AutoVoidRequestQueueService autoVoidRequestQueueService)
    {
        this.requestDetailService = requestDetailService;
        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
    }

    public async Task<(bool IsValid, string RequestIdsChangedAction)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue)
    {
        var requestIdList = queue.RequestIds.Split(",");
        var requestIdsChangedAction = await GetRequestIdsChangedAction(requestIdList, queue.Action);

        if (requestIdList.Length == requestIdsChangedAction.Count)
        {
            var queueErrorMessage = $"Requests ({queue.RequestIds}) Action was edited.";
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
            return (false, string.Empty);
        }

        queue.RequestIds = string.Join(",", requestIdList.Except(requestIdsChangedAction));

        return (true, string.Join(",", requestIdsChangedAction));
    }

    private async Task<List<string>> GetRequestIdsChangedAction(IEnumerable<string> requestIdList, string actionNameQueue)
    {
        var requestIdsChangedAction = new List<string>();

        foreach (var requestId in requestIdList)
        {
            var (request, _) = await requestDetailService.GetRequestById(int.Parse(requestId));
            if (request.Any())
            {
                var actionNameRequest = VoidActionTypeConstant.VoidActionList.ContainsKey(request.First().ActionId) ?
                    VoidActionTypeConstant.VoidActionList[request.First().ActionId] : string.Empty;
                if (!actionNameRequest.Equals(actionNameQueue, StringComparison.OrdinalIgnoreCase))
                {
                    requestIdsChangedAction.Add(requestId);
                }
            }
        }

        return requestIdsChangedAction;
    }
}