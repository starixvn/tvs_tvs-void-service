using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface GetTicketsStage
{
    Task<(bool IsContinueFlow, GroupTicketsInRequest OutputData)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData);
}

public class GetTicketsStageImp(RequestTicketService requestTicketService) : GetTicketsStage
{
    public async Task<(bool, GroupTicketsInRequest)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData)
    {
        var getTicketsResult = await GetTicketsInRequest(siteDbGroupId, queue);
        logData.Add($"{nameof(GetTicketsStage)} - SingleCount: {getTicketsResult.SingleTickets.Count()} " +
                    $"- ParlayCount: {getTicketsResult.ParlayTickets.Count()} - CricketCount: {getTicketsResult.CricketTickets.Count()} " +
                    $"- CancelCOCount: {getTicketsResult.CancelCOTickets.Count()}");

        if (!getTicketsResult.SingleTickets.Any() && !getTicketsResult.ParlayTickets.Any() && !getTicketsResult.CricketTickets.Any() && !getTicketsResult.CancelCOTickets.Any())
        {
            return (false, null);
        }

        return (true, new GroupTicketsInRequest
        {
            SingleTickets = getTicketsResult.SingleTickets,
            ParlayTickets = getTicketsResult.ParlayTickets,
            CricketTickets = getTicketsResult.CricketTickets,
            CancelCOTickets = getTicketsResult.CancelCOTickets
        });
    }

    private async Task<GroupTicketsInRequest> GetTicketsInRequest(int siteDbGroupId, AutoVoidRequestQueueModel queue)
    {
        var getTicketsInRequest = await requestTicketService.GetTicketsByRequestsAndSiteDbGroupId(siteDbGroupId, queue.RequestIds);

        var actionAutoVoidInstants = new List<string>
        {
            AutoVoidConstant.QueueActionVoidInstant,
            AutoVoidConstant.QueueActionVoidInstantNoPolicy,
            AutoVoidConstant.QueueActionVoidInstantWithPolicy,
            AutoVoidConstant.QueueRecoverInstant
        };

        if (actionAutoVoidInstants.Contains(queue.Action) && queue.MatchId > 0)
        {
            var requestId = int.Parse(queue.RequestIds);
            var singleTicketsInMatch = getTicketsInRequest.SingleTickets.Where(x => x.MatchId == queue.MatchId && x.RequestId == requestId);
            var parlayTicketsInMatch = getTicketsInRequest.ParlayTickets.Where(x => x.MatchId == queue.MatchId && x.RequestId == requestId);
            var cricketTicketsInMatch = getTicketsInRequest.CricketTickets.Where(x => x.MatchId == queue.MatchId && x.RequestId == requestId);
            var cancelCashOutTicketsInMatch = getTicketsInRequest.CancelCOTickets.Where(x => x.MatchId == queue.MatchId && x.RequestId == requestId);

            return new GroupTicketsInRequest
            {
                SingleTickets = singleTicketsInMatch,
                ParlayTickets = parlayTicketsInMatch,
                CricketTickets = cricketTicketsInMatch,
                CancelCOTickets = cancelCashOutTicketsInMatch
            };
        }

        var actionAutoWinningResultKeyed = new List<string>
        {
            AutoVoidConstant.QueueActionCal,
            AutoVoidConstant.QueueActionVoidResultKeyed
        };

        if (actionAutoWinningResultKeyed.Contains(queue.Action) && queue.MatchId > 0)
        {
            var singleTicketsInMatch = getTicketsInRequest.SingleTickets.Where(x => x.MatchId == queue.MatchId);
            var parlayTicketsInMatch = getTicketsInRequest.ParlayTickets.Where(x => x.MatchId == queue.MatchId);
            var cricketTicketsInMatch = getTicketsInRequest.CricketTickets.Where(x => x.MatchId == queue.MatchId);
            var cancelCashOutTicketsInMatch = getTicketsInRequest.CancelCOTickets.Where(x => x.MatchId == queue.MatchId);
            //// CancelCO not support yet in auto winning
            return new GroupTicketsInRequest
            {
                SingleTickets = singleTicketsInMatch,
                ParlayTickets = parlayTicketsInMatch,
                CricketTickets = cricketTicketsInMatch,
                CancelCOTickets = cancelCashOutTicketsInMatch
            };
        }

        return getTicketsInRequest;
    }
}