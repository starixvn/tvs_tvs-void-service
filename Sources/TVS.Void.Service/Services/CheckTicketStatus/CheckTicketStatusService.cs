using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckTicketStatus;

public interface CheckTicketStatusService
{
    Task Execute();
}

public class CheckTicketStatusServiceImp : CheckTicketStatusService
{
    private readonly RequestTicketService requestTicketService;
    private readonly CheckTicketsForAutoVoidWinningStage checkTicketsForAutoVoidWinningStage;
    private readonly CheckTicketsForAutoVoidAllResultKeyedStage checkTicketsForAutoVoidAllResultKeyedStage;

    private readonly LoggingService loggingService;

    public CheckTicketStatusServiceImp(RequestTicketService requestTicketService, CheckTicketsForAutoVoidWinningStage checkTicketsForAutoVoidWinningStage,
        CheckTicketsForAutoVoidAllResultKeyedStage checkTicketsForAutoVoidAllResultKeyedStage, LoggingService loggingService)
    {
        this.requestTicketService = requestTicketService;
        this.checkTicketsForAutoVoidWinningStage = checkTicketsForAutoVoidWinningStage;
        this.checkTicketsForAutoVoidAllResultKeyedStage = checkTicketsForAutoVoidAllResultKeyedStage;
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
        var getTicketsResult = await requestTicketService.GetTicketInAllRequests();

        var autoVoidWinningGroup = GetGroupTicketsByVoidAction(VoidActionTypeConstant.AutoVoidWinningId, getTicketsResult);
        await checkTicketsForAutoVoidWinningStage.Execute(logData, autoVoidWinningGroup);

        //// Currently support Auto Void Result keyed with 1 match
        var autoVoidAllResultKeyedGroup = GetGroupTicketsByVoidAction(VoidActionTypeConstant.AutoVoidAllResultKeyedId, getTicketsResult);
        await checkTicketsForAutoVoidAllResultKeyedStage.Execute(logData, autoVoidAllResultKeyedGroup);

        await loggingService.InfoAppendNewLineAsync(logData);
    }

    private static GroupTicketsInRequest GetGroupTicketsByVoidAction(int voidActionId, GroupTicketsInRequest getTicketsResult)
    {
        var singleTickets = getTicketsResult.SingleTickets.Where(x => x.VoidActionId == voidActionId).ToList();
        UpdateMatchIdByAutoVoidWinningMultiMatch(singleTickets);

        var parlayTickets = getTicketsResult.ParlayTickets.Where(x => x.VoidActionId == voidActionId).ToList();
        UpdateMatchIdByAutoVoidWinningMultiMatch(parlayTickets);

        var cricketTickets = getTicketsResult.CricketTickets.Where(x => x.VoidActionId == voidActionId).ToList();
        UpdateMatchIdByAutoVoidWinningMultiMatch(cricketTickets);
        return new GroupTicketsInRequest
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets
        };
    }

    private static void UpdateMatchIdByAutoVoidWinningMultiMatch(IEnumerable<RequestDetailTicketModel> tickets)
    {
        foreach (var ticket in tickets)
        {
            var isMultiMatchesInRequestTicket = ticket.IssueTypeId == IssueTypeConstant.AbnormalBet;
            ticket.MatchId = isMultiMatchesInRequestTicket ? ticket.MatchId : 0;
        }
    }

    private async Task HandleException(IList<string> logData, Exception exception)
    {
        await loggingService.InfoAppendNewLineAsync(logData);
        await loggingService.ErrorAsync($"{nameof(CheckTicketStatusService)} Error", exception);
    }
}