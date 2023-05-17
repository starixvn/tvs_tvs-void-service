using System;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface CheckVoidInstantRequestQueueValidStage
{
    Task<(bool IsValid, int IssueTypeId)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue);
}

public class CheckVoidInstantRequestQueueValidStageImp : CheckVoidInstantRequestQueueValidStage
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly RequestDetailService requestDetailService;

    public CheckVoidInstantRequestQueueValidStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService)
    {
        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        this.requestDetailService = requestDetailService;
    }

    public async Task<(bool IsValid, int IssueTypeId)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue)
    {
        var requestIdList = queue.RequestIds.Split(",");
        if (requestIdList.Length > 1)
        {
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "Too many RequestIds.");
            return (false, 0);
        }

        var (request, _) = await requestDetailService.GetRequestById(int.Parse(requestIdList.First()));
        if (!request.Any())
        {
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "RequestId invalid.");
            return (false, 0);
        }

        if (!request.First().ActionName.Equals(queue.Action, StringComparison.OrdinalIgnoreCase))
        {
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "Request Action was edited.");
            return (false, 0);
        }

        return (true, request.First().IssueTypeId);
    }
}