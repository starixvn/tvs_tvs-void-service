using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
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
        var requestIdsChangedAction = new List<string>();

        var tasks = requestIdList.Select(async requestId =>
        {
            var (request, _) = await requestDetailService.GetRequestById(int.Parse(requestId));
            if (request.Any() && !request.First().ActionName.Equals(queue.Action, StringComparison.OrdinalIgnoreCase))
            {
                requestIdsChangedAction.Add(requestId);
            }
        });

        await Task.WhenAll(tasks);

        if (requestIdList.Length == requestIdsChangedAction.Count)
        {
            var queueErrorMessage = $"Requests ({queue.RequestIds}) Action was edited.";
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
            return (false, string.Empty);
        }

        queue.RequestIds = string.Join(",", requestIdList.Except(requestIdsChangedAction));

        return (true, string.Join(",", requestIdsChangedAction));
    }
}