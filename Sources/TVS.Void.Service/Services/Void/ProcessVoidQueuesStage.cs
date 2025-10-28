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

public class ProcessVoidQueuesStageImp(
    TicketVoidingQueueService ticketVoidingQueueService,
    NetPositionService netPositionService,
    RequestTicketService requestTicketService,
    HybridParlayTicketService hybridParlayTicketService)
    : ProcessVoidQueuesStage
{
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
            VoidingTypeConstant.HybridParlay => await ExecuteQueueForHybridParlayTickets(siteDbGroupId, queue, true),
            VoidingTypeConstant.RecoverHybridParlay => await ExecuteQueueForHybridParlayTickets(siteDbGroupId, queue, false),
            _ => await ticketVoidingQueueService.ProcessVoidQueue(siteDbGroupId, queue.QueueId, null, null)
        };
    }

    private async Task<TicketVoidingQueueModel> ExecuteQueueForCricketTickets(int siteDbGroupId, TicketVoidingQueueModel queue, string actionName)
    {
        var isCalledNetPosition = await netPositionService.UpdateNetPosition(siteDbGroupId, queue.TransIds, actionName);
        var voidResult = await ticketVoidingQueueService.ProcessVoidQueue(siteDbGroupId, queue.QueueId, isCalledNetPosition, null);

        if (voidResult.StatusId == ExecuteStatus.Failed && isCalledNetPosition)
        {
            await netPositionService.RollBackNetPosition(siteDbGroupId, queue.TransIds);
        }

        return voidResult;
    }

    private async Task<TicketVoidingQueueModel> ExecuteQueueForHybridParlayTickets(int siteDbGroupId, TicketVoidingQueueModel queue, bool isVoid)
    {
        var ticketsDetail = await requestTicketService.GetTicketsDetail(siteDbGroupId, queue.TransIds);
        var isCalledJackTimeHybridParlay = await hybridParlayTicketService.CallApi(ticketsDetail, isVoid);
        return await ticketVoidingQueueService.ProcessVoidQueue(siteDbGroupId, queue.QueueId, null, isCalledJackTimeHybridParlay);
    }
}