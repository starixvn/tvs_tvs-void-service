using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
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
        var getTicketsResult = await GetTicketsInRequest(siteDbGroupId, queue);
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

    private async Task<GroupTicketsInRequest> GetTicketsInRequest(int siteDbGroupId, AutoVoidRequestQueueModel queue)
    {
        var getTicketsInRequest = await requestTicketService.GetTicketsByRequestsAndSiteDbGroupId(siteDbGroupId, queue.RequestIds);

        if (queue.Action.Equals(AutoVoidConstant.QueueActionVoidInstant, StringComparison.InvariantCultureIgnoreCase) && queue.MatchId > 0)
        {
            var requestId = int.Parse(queue.RequestIds);
            var singleTicketsInMatch = getTicketsInRequest.SingleTickets.Where(x => x.MatchId == queue.MatchId && x.RequestId == requestId);
            var parlayTicketsInMatch = getTicketsInRequest.ParlayTickets.Where(x => x.MatchId == queue.MatchId && x.RequestId == requestId);
            var cricketTicketsInMatch = getTicketsInRequest.CricketTickets.Where(x => x.MatchId == queue.MatchId && x.RequestId == requestId);

            return new GroupTicketsInRequest
            {
                SingleTickets = singleTicketsInMatch,
                ParlayTickets = parlayTicketsInMatch,
                CricketTickets = cricketTicketsInMatch
            };
        }

        if (queue.Action.Equals(AutoVoidConstant.QueueActionCal, StringComparison.InvariantCultureIgnoreCase) && queue.MatchId > 0)
        {
            var singleTicketsInMatch = getTicketsInRequest.SingleTickets.Where(x => x.MatchId == queue.MatchId);
            var parlayTicketsInMatch = getTicketsInRequest.ParlayTickets.Where(x => x.MatchId == queue.MatchId);
            var cricketTicketsInMatch = getTicketsInRequest.CricketTickets.Where(x => x.MatchId == queue.MatchId);

            return new GroupTicketsInRequest
            {
                SingleTickets = singleTicketsInMatch,
                ParlayTickets = parlayTicketsInMatch,
                CricketTickets = cricketTicketsInMatch
            };
        }

        return getTicketsInRequest;
    }
}