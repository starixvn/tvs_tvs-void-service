using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface AutoVoidWinningProcessStage
{
    Task Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData);
}

public class AutoVoidWinningProcessStageImp : AutoVoidWinningProcessStage
{
    private readonly GetTicketsStage getTicketsStage;
    private readonly CheckTicketValidationStage checkTicketAllowToVoidStage;
    private readonly ReCalculateTicketsStage reCalculateTicketsStage;
    private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly LoggingService loggingService;

    public AutoVoidWinningProcessStageImp(GetTicketsStage getTicketsStage, CheckTicketValidationStage checkTicketAllowToVoidStage, ReCalculateTicketsStage reCalculateTicketsStage,
        InsertPreVoidingDataStage insertPreVoidingDataStage, AutoVoidRequestQueueService autoVoidRequestQueueService, LoggingService loggingService)
    {
        this.getTicketsStage = getTicketsStage;
        this.checkTicketAllowToVoidStage = checkTicketAllowToVoidStage;
        this.reCalculateTicketsStage = reCalculateTicketsStage;
        this.insertPreVoidingDataStage = insertPreVoidingDataStage;
        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        this.loggingService = loggingService;
    }

    public async Task Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData)
    {
        var getTicketsResult = await getTicketsStage.Execute(siteDbGroupId, queue, logData);
        if (!getTicketsResult.IsContinueFlow)
        {
            return;
        }

        var checkTicketResult = await checkTicketAllowToVoidStage.Execute(siteDbGroupId, getTicketsResult.OutputData.SingleTickets,
            getTicketsResult.OutputData.ParlayTickets, getTicketsResult.OutputData.CricketTickets, logData);

        await reCalculateTicketsStage.Execute(siteDbGroupId, checkTicketResult.SingleTicketsResettle,
            checkTicketResult.ParlayTicketsResettle, checkTicketResult.CricketTicketsResettle, logData);

        var queueErrorMessage = await insertPreVoidingDataStage.Execute(siteDbGroupId, queue,
            checkTicketResult.ValidVoidSingleTickets, checkTicketResult.ValidVoidParlayTickets, checkTicketResult.ValidVoidCricketTickets,
            Enumerable.Empty<RequestDetailTicketModel>(), logData);

        await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
        await WriteLog(logData);
    }

    private async Task WriteLog(IList<string> logData)
    {
        logData.Add($"ServerTime: {DateTimeOffset.Now:MM/dd/yyyy HH:mm:ss.fff}");
        await loggingService.InfoAppendNewLineAsync(logData);
    }
}