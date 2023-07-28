using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;

namespace TVS.Void.Service.Services.CheckTicketStatus;

public interface CheckTicketStatusService
{
    Task Execute();
}

public class CheckTicketStatusServiceImp : CheckTicketStatusService
{
    private readonly RequestTicketService requestTicketService;
    private readonly InsertAutoVoidQueueStage insertAutoVoidQueueStage;
    private readonly LoggingService loggingService;

    public CheckTicketStatusServiceImp(RequestTicketService requestTicketService, InsertAutoVoidQueueStage insertAutoVoidQueueStage, LoggingService loggingService)
    {
        this.requestTicketService = requestTicketService;
        this.insertAutoVoidQueueStage = insertAutoVoidQueueStage;
        this.loggingService = loggingService;
    }

    public async Task Execute()
    {
        var logData = new List<string>();
        try
        {
            await ExecuteNormalFlow(logData);
        }
        catch (Exception e)
        {
            await HandleException(logData, e);
        }
    }

    private async Task ExecuteNormalFlow(IList<string> logData)
    {
        var (singleTickets, parlayTickets, cricketTickets) = await requestTicketService.GetTicketInAllRequests();
        if (singleTickets.Any() || parlayTickets.Any() || cricketTickets.Any())
        {
            logData.Add(
                $"{nameof(CheckTicketStatusService)} - {nameof(singleTickets)}.Count: {singleTickets.Count()} - "
                + $"{nameof(parlayTickets)}.Count: {parlayTickets.Count()} - {nameof(cricketTickets)}.Count: {cricketTickets.Count()}");
            var broadcastSessionId = Guid.NewGuid().ToString();
            foreach (var server in SiteDbGroupConstant.ServerList)
            {
                var singleTicketsInServer = singleTickets.Where(x => x.SiteDbGroupId == server.Id);
                var parlayTicketsInServer = parlayTickets.Where(x => x.SiteDbGroupId == server.Id);
                var cricketTicketsInServer = cricketTickets.Where(x => x.SiteDbGroupId == server.Id);

                var checkResult = await requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(server.Id, singleTicketsInServer, parlayTicketsInServer, cricketTicketsInServer);
                await insertAutoVoidQueueStage.Execute(server.Id, checkResult.ValidRequests, broadcastSessionId, logData);
                await requestTicketService.UpdateIsProcessed(checkResult.InvalidSingleTickets, checkResult.InvalidParlayTickets, checkResult.InvalidCricketTickets);
                logData.Add($"[{server.Id}]{nameof(requestTicketService.CheckStatusTicketForInsertAutoVoidQueue)} - {JsonSerializer.Serialize(checkResult)}");
            }

            logData.Add($"ServerTime: {DateTime.Now:MM/dd/yyyy HH:mm:ss.fff}");
            await loggingService.InfoAppendNewLineAsync(logData);
        }
    }

    private async Task HandleException(IList<string> logData, Exception exception)
    {
        await loggingService.InfoAppendNewLineAsync(logData);
        await loggingService.ErrorAsync($"{nameof(CheckTicketStatusService)} Error", exception);
    }
}