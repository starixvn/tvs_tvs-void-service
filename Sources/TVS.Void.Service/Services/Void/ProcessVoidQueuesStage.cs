using System.Collections.Generic;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void;

public interface ProcessVoidQueuesStage
{
    Task<IList<TicketVoidingQueueModel>> Execute(int siteDbGroupId, IEnumerable<TicketVoidingQueueModel> voidingQueues);
}

public class ProcessVoidQueuesStageImp : ProcessVoidQueuesStage
{
    private readonly TicketVoidingQueueService ticketVoidingQueueService;
    private readonly NetPositionService netPositionService;

    public ProcessVoidQueuesStageImp(TicketVoidingQueueService ticketVoidingQueueService, NetPositionService netPositionService)
    {
        this.ticketVoidingQueueService = ticketVoidingQueueService;
        this.netPositionService = netPositionService;
    }

    public async Task<IList<TicketVoidingQueueModel>> Execute(int siteDbGroupId, IEnumerable<TicketVoidingQueueModel> voidingQueues)
    {
        var processResult = new List<TicketVoidingQueueModel>();
        foreach (var queue in voidingQueues)
        {
            if (queue.StatusId is ExecuteStatus.Done or ExecuteStatus.Failed)
            {
                processResult.Add(queue);
                continue;
            }

            var voidResult = await ExecuteSingleQueue(siteDbGroupId, queue);
            processResult.Add(voidResult);
        }

        return processResult;
    }

    private async Task<TicketVoidingQueueModel> ExecuteSingleQueue(int siteDbGroupId, TicketVoidingQueueModel queue)
    {
        return queue.VoidingTypeId switch
        {
            VoidingTypeConstant.Cricket => await ExecuteQueueForCricketTickets(siteDbGroupId, queue, NetPositionConstant.ActionVoid),
            VoidingTypeConstant.RecoverCricket => await ExecuteQueueForCricketTickets(siteDbGroupId, queue, NetPositionConstant.ActionRecover),
            _ => await ticketVoidingQueueService.ProcessVoidQueue(siteDbGroupId, queue.QueueId, null)
        };
    }

    private async Task<TicketVoidingQueueModel> ExecuteQueueForCricketTickets(int siteDbGroupId, TicketVoidingQueueModel queue, string actionName)
    {
        var isCalledNetPosition = await netPositionService.UpdateNetPosition(siteDbGroupId, queue.TransIds, actionName);
        var voidResult = await ticketVoidingQueueService.ProcessVoidQueue(siteDbGroupId, queue.QueueId, isCalledNetPosition);

        if (voidResult.StatusId == ExecuteStatus.Failed && isCalledNetPosition)
        {
            await netPositionService.RollBackNetPosition(siteDbGroupId, queue.TransIds);
        }

        return voidResult;
    }
}