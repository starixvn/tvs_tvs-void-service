using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface GetTicketsStage
{
    Task<(bool IsContinueFlow, GroupTicketsInRequest OutputData)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData);
}

public class GetTicketsStageImp : GetTicketsStage
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly RequestTicketService requestTicketService;

    public GetTicketsStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestTicketService requestTicketService)
    {
        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        this.requestTicketService = requestTicketService;
    }

    public async Task<(bool, GroupTicketsInRequest)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData)
    {
        var getTicketsResult = await requestTicketService.GetTicketsByRequestsAndSiteDbGroupId(siteDbGroupId, queue.RequestIds);
        logData.Add($"{nameof(GetTicketsStage)} - SingleCount: {getTicketsResult.SingleTickets.Count()} " +
                    $"- ParlayCount: {getTicketsResult.ParlayTickets.Count()} - CricketCount: {getTicketsResult.CricketTickets.Count()}");

        if (!getTicketsResult.SingleTickets.Any() && !getTicketsResult.ParlayTickets.Any() && !getTicketsResult.CricketTickets.Any())
        {
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "No ticket found.");
            return (false, null);
        }

        return (true, new GroupTicketsInRequest
        {
            SingleTickets = getTicketsResult.SingleTickets,
            ParlayTickets = getTicketsResult.ParlayTickets,
            CricketTickets = getTicketsResult.CricketTickets
        });
    }
}