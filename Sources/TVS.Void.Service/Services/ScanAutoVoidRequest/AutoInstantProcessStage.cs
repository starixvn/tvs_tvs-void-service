using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface AutoInstantProcessStage
{
    Task Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData);
}

public class AutoInstantProcessStageImp : AutoInstantProcessStage
{
    private readonly CheckVoidInstantRequestQueueValidStage checkRequestQueueValidStage;
    private readonly GetTicketsStage getTicketsStage;
    private readonly CheckTicketValidationStage checkTicketValidationStage;
    private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly CreateVoidDataXmlStage createVoidDataXmlStage;
    private readonly LoggingService loggingService;

    public AutoInstantProcessStageImp(CheckVoidInstantRequestQueueValidStage checkRequestQueueValidStage, GetTicketsStage getTicketsStage,
        CheckTicketValidationStage checkTicketValidationStage, InsertPreVoidingDataStage insertPreVoidingDataStage,
        AutoVoidRequestQueueService autoVoidRequestQueueService, CreateVoidDataXmlStage createVoidDataXmlStage, LoggingService loggingService)
    {
        this.checkRequestQueueValidStage = checkRequestQueueValidStage;
        this.getTicketsStage = getTicketsStage;
        this.checkTicketValidationStage = checkTicketValidationStage;
        this.insertPreVoidingDataStage = insertPreVoidingDataStage;
        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        this.createVoidDataXmlStage = createVoidDataXmlStage;
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

        CheckTicketValidationOutput validationResult;
        //// Recheck for issue type have Cricket, Archive (some case ticket moved...), CancelCO and need some ticket info in bettrans
        if (IsNeedToReCheckVoidValidation(checkQueueResult.IssueTypeId))
        {
            validationResult = await checkTicketValidationStage.ExecuteForVoid(siteDbGroupId, getTicketsResult.OutputData.SingleTickets,
                getTicketsResult.OutputData.ParlayTickets, getTicketsResult.OutputData.CricketTickets, getTicketsResult.OutputData.CancelCOTickets, logData);
        }
        else if (IsRecoverAction(queue.Action))
        {
            validationResult = await checkTicketValidationStage.ExecuteForRecover(siteDbGroupId, getTicketsResult.OutputData.SingleTickets,
                getTicketsResult.OutputData.ParlayTickets, getTicketsResult.OutputData.CricketTickets, logData);
        }
        else
        {
            validationResult = new CheckTicketValidationOutput
            {
                VoidSingleTickets = getTicketsResult.OutputData.SingleTickets,
                VoidParlayTickets = getTicketsResult.OutputData.ParlayTickets,
                VoidCricketTickets = getTicketsResult.OutputData.CricketTickets
            };
        }

        var groupTickets = insertPreVoidingDataStage.MapVoidingTypeToTickets(validationResult);
        var voidDataXml = createVoidDataXmlStage.Execute(!IsRecoverAction(queue.Action), validationResult);
        var queueErrorMessage = await insertPreVoidingDataStage.Execute(siteDbGroupId, queue, groupTickets, voidDataXml, logData);
        await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
        await WriteLog(logData);
    }

    private static bool IsNeedToReCheckVoidValidation(int issueTypeId)
    {
        var issueTypesReCheckValidation = new List<int>
        {
            IssueTypeConstant.AbnormalBet, // AB need recheck, because it support void 14
            IssueTypeConstant.AbnormalAccount,
            IssueTypeConstant.ReducedOver,
            IssueTypeConstant.DuplicateTickets,
            IssueTypeConstant.NotParticipate,
            IssueTypeConstant.EarlyKickOffTime,
            IssueTypeConstant.WrongKickOffTime,
            IssueTypeConstant.Abandoned,
            IssueTypeConstant.WrongMarketOffered,
            IssueTypeConstant.TechnicalError
        };
        return issueTypesReCheckValidation.Contains(issueTypeId);
    }

    private static bool IsRecoverAction(string actionName)
    {
        return actionName.Equals(AutoVoidConstant.QueueRecoverInstant, StringComparison.OrdinalIgnoreCase);
    }

    private async Task WriteLog(IList<string> logData)
    {
        logData.Add($"ServerTime: {DateTimeOffset.Now:MM/dd/yyyy HH:mm:ss.fff}");
        await loggingService.InfoAppendNewLineAsync(logData);
    }
}