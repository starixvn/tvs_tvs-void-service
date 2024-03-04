using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckTicketStatus;

public interface CheckTicketsForAutoVoidWinningStage
{
    Task Execute(IList<string> logData, GroupTicketsInRequest groupTickets);
}

public class CheckTicketsForAutoVoidWinningStageImp : CheckTicketsForAutoVoidWinningStage
{
    private readonly RequestTicketService requestTicketService;
    private readonly InsertAutoVoidQueueStage insertAutoVoidQueueStage;

    public CheckTicketsForAutoVoidWinningStageImp(RequestTicketService requestTicketService, InsertAutoVoidQueueStage insertAutoVoidQueueStage)
    {
        this.requestTicketService = requestTicketService;
        this.insertAutoVoidQueueStage = insertAutoVoidQueueStage;
    }

    public async Task Execute(IList<string> logData, GroupTicketsInRequest groupTickets)
    {
        if (groupTickets.SingleTickets.Any() || groupTickets.ParlayTickets.Any() || groupTickets.CricketTickets.Any())
        {
            logData.Add($"{nameof(CheckTicketsForAutoVoidWinningStage)} - {nameof(groupTickets.SingleTickets)}.Count: {groupTickets.SingleTickets.Count()} - "
                        + $"{nameof(groupTickets.ParlayTickets)}.Count: {groupTickets.ParlayTickets.Count()} - {nameof(groupTickets.CricketTickets)}.Count: {groupTickets.CricketTickets.Count()}");
            var broadcastSessionId = Guid.NewGuid().ToString();
            foreach (var siteDbGroupId in SiteDbGroupConstant.ServerList.Select(x => x.Id))
            {
                var singleTicketsInServer = groupTickets.SingleTickets.Where(x => x.SiteDbGroupId == siteDbGroupId);
                var parlayTicketsInServer = groupTickets.ParlayTickets.Where(x => x.SiteDbGroupId == siteDbGroupId);
                var cricketTicketsInServer = groupTickets.CricketTickets.Where(x => x.SiteDbGroupId == siteDbGroupId);

                var checkResult = await requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(siteDbGroupId, singleTicketsInServer, parlayTicketsInServer, cricketTicketsInServer);
                await insertAutoVoidQueueStage.ExecuteForAutoVoidWinning(siteDbGroupId, checkResult.ValidRequests, broadcastSessionId, logData);
                await requestTicketService.UpdateIsProcessed(checkResult.InvalidSingleTickets, checkResult.InvalidParlayTickets, checkResult.InvalidCricketTickets);
                logData.Add($"[{siteDbGroupId}]{nameof(requestTicketService.CheckStatusTicketForInsertAutoVoidQueue)} - {JsonSerializer.Serialize(checkResult)}");
            }

            logData.Add($"ServerTime: {DateTimeOffset.Now:MM/dd/yyyy HH:mm:ss.fff}");
            logData.Add("------------------");
        }
    }
}