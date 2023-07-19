using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface AutoVoidInstantProcessStage
{
    Task Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData);
}

public class AutoVoidInstantProcessStageImp : AutoVoidInstantProcessStage
{
    private readonly CheckVoidInstantRequestQueueValidStage checkRequestQueueValidStage;
    private readonly GetTicketsStage getTicketsStage;
    private readonly CheckTicketAllowToVoidStage checkTicketAllowToVoidStage;
    private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly LoggingService loggingService;

    public AutoVoidInstantProcessStageImp(CheckVoidInstantRequestQueueValidStage checkRequestQueueValidStage, GetTicketsStage getTicketsStage,
        CheckTicketAllowToVoidStage checkTicketAllowToVoidStage, InsertPreVoidingDataStage insertPreVoidingDataStage,
        AutoVoidRequestQueueService autoVoidRequestQueueService, LoggingService loggingService)
    {
        this.checkRequestQueueValidStage = checkRequestQueueValidStage;
        this.getTicketsStage = getTicketsStage;
        this.checkTicketAllowToVoidStage = checkTicketAllowToVoidStage;
        this.insertPreVoidingDataStage = insertPreVoidingDataStage;

        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        this.loggingService = loggingService;
    }

    public async Task Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData)
    {
        var checkQueueResult = await checkRequestQueueValidStage.Execute(siteDbGroupId, queue);
        if (!checkQueueResult.IsValid)
        {
            return;
        }

        var getTicketsResult = await getTicketsStage.Execute(siteDbGroupId, queue, logData);
        if (!getTicketsResult.IsContinueFlow)
        {
            return;
        }

        var validVoidSingleTickets = getTicketsResult.OutputData.SingleTickets;
        var validVoidParlayTickets = getTicketsResult.OutputData.ParlayTickets;
        var validVoidCricketTickets = getTicketsResult.OutputData.CricketTickets;
        var validVoidSingleTickets14 = Enumerable.Empty<RequestDetailTicketModel>();

        if (checkQueueResult.IssueTypeId is IssueTypeConstant.AbnormalAccount or IssueTypeConstant.ReducedOver)
        {
            var checkTicketResult = await checkTicketAllowToVoidStage.Execute(siteDbGroupId, getTicketsResult.OutputData.SingleTickets,
                getTicketsResult.OutputData.ParlayTickets, getTicketsResult.OutputData.CricketTickets, logData);

            validVoidSingleTickets = checkTicketResult.ValidVoidSingleTickets;
            validVoidParlayTickets = checkTicketResult.ValidVoidParlayTickets;
            validVoidCricketTickets = checkTicketResult.ValidVoidCricketTickets;
            validVoidSingleTickets14 = checkTicketResult.ValidVoidSingleTickets14;
        }
        var queueErrorMessage = await insertPreVoidingDataStage.Execute(siteDbGroupId, queue, validVoidSingleTickets,
            validVoidParlayTickets, validVoidCricketTickets, validVoidSingleTickets14, logData);
        await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
        await WriteLog(logData);
    }

    private async Task WriteLog(IList<string> logData)
    {
        logData.Add($"ServerTime: {DateTime.Now:MM/dd/yyyy HH:mm:ss.fff}");
        await loggingService.InfoAppendNewLineAsync(logData);
    }
}