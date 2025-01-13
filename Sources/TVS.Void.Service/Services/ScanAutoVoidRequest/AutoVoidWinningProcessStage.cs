using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface AutoVoidWinningProcessStage
{
    Task Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData);
}

public class AutoVoidWinningProcessStageImp(
    CheckTicketValidationStage checkTicketAllowToVoidStage,
    ReCalculateTicketsStage reCalculateTicketsStage,
    InsertPreVoidingDataStage insertPreVoidingDataStage,
    AutoVoidRequestQueueService autoVoidRequestQueueService,
    CreateVoidDataXmlStage createVoidDataXmlStage,
    LoggingService loggingService,
    AutoVoidWinningBeforeVoidService autoVoidWinningBeforeVoidService)
    : AutoVoidWinningProcessStage
{
    public async Task Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData)
    {
        var checkQueueResult = await autoVoidWinningBeforeVoidService.CheckVoidWinningRequestQueueValid(siteDbGroupId, queue);
        if (!checkQueueResult.IsValid)
        {
            return;
        }

        var getTicketsResult = await autoVoidWinningBeforeVoidService.GetTickets(siteDbGroupId, queue, logData);
        if (!getTicketsResult.IsContinueFlow)
        {
            var errorMessage = GetErrorMessage(checkQueueResult.RequestIdsChangedAction, queue.RequestIds, "No ticket found.");
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, errorMessage);
            return;
        }

        var checkTicketResult = await checkTicketAllowToVoidStage.ExecuteForVoid(siteDbGroupId, getTicketsResult.OutputData.SingleTickets, getTicketsResult.OutputData.ParlayTickets,
            getTicketsResult.OutputData.CricketTickets, getTicketsResult.OutputData.CancelCOTickets, logData);
        await reCalculateTicketsStage.Execute(siteDbGroupId, checkTicketResult.SingleTicketsResettle,
            checkTicketResult.ParlayTicketsResettle, checkTicketResult.CricketTicketsResettle, logData);

        var groupTickets = insertPreVoidingDataStage.MapVoidingTypeToTickets(checkTicketResult);
        var voidDataXml = createVoidDataXmlStage.Execute(true, checkTicketResult);
        var queueErrorMessage = await insertPreVoidingDataStage.Execute(siteDbGroupId, queue, groupTickets, voidDataXml, logData);

        var finalErrorMessage = GetErrorMessage(checkQueueResult.RequestIdsChangedAction, queue.RequestIds, queueErrorMessage);
        await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, finalErrorMessage);
        await WriteLog(logData);
    }

    private static string GetErrorMessage(string requestIdsChangedAction, string requestIdsGetTicket, string errMsgGetTickets)
    {
        var messages = new List<string>();

        if (!string.IsNullOrWhiteSpace(requestIdsChangedAction))
        {
            messages.Add($"Requests ({requestIdsChangedAction}) Action was edited.");
        }

        if (!string.IsNullOrWhiteSpace(errMsgGetTickets))
        {
            messages.Add($"Requests ({requestIdsGetTicket}) {errMsgGetTickets}");
        }

        return string.Join(" ", messages);
    }

    private async Task WriteLog(IList<string> logData)
    {
        logData.Add($"ServerTime: {DateTimeOffset.Now:MM/dd/yyyy HH:mm:ss.fff}");
        await loggingService.InfoAppendNewLineAsync(logData);
    }
}