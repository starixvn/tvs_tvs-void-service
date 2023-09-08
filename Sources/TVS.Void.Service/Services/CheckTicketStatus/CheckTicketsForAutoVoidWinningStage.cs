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
            foreach (var server in SiteDbGroupConstant.ServerList)
            {
                var singleTicketsInServer = groupTickets.SingleTickets.Where(x => x.SiteDbGroupId == server.Id);
                var parlayTicketsInServer = groupTickets.ParlayTickets.Where(x => x.SiteDbGroupId == server.Id);
                var cricketTicketsInServer = groupTickets.CricketTickets.Where(x => x.SiteDbGroupId == server.Id);

                var checkResult = await requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(server.Id, singleTicketsInServer, parlayTicketsInServer, cricketTicketsInServer);
                await insertAutoVoidQueueStage.ExecuteForAutoVoidWinning(server.Id, checkResult.ValidRequests, broadcastSessionId, logData);
                await requestTicketService.UpdateIsProcessed(checkResult.InvalidSingleTickets, checkResult.InvalidParlayTickets, checkResult.InvalidCricketTickets);
                logData.Add($"[{server.Id}]{nameof(requestTicketService.CheckStatusTicketForInsertAutoVoidQueue)} - {JsonSerializer.Serialize(checkResult)}");
            }

            logData.Add($"ServerTime: {DateTime.Now:MM/dd/yyyy HH:mm:ss.fff}");
            logData.Add("------------------");
        }
    }
}