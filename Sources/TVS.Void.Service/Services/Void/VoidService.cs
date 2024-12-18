using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void;

public interface VoidService
{
    Task Execute(int siteDbGroupId);
}

public class VoidServiceImp(
    TicketVoidingQueueService ticketVoidingQueueService,
    RequestDetailService requestDetailService,
    ProcessVoidQueuesStage processVoidQueuesStage,
    InsertAfterVoidDataStage insertAfterVoidDataStage,
    NotificationService notificationService,
    LoggingService loggingService)
    : VoidService
{
    public async Task Execute(int siteDbGroupId)
    {
        var voidingQueues = await ticketVoidingQueueService.GetQueues(siteDbGroupId, null);
        if (!voidingQueues.Any())
        {
            return;
        }

        await ExecuteWhenHaveQueues(siteDbGroupId, voidingQueues);
    }

    private async Task ExecuteWhenHaveQueues(int siteDbGroupId, IList<TicketVoidingQueueModel> voidingQueues)
    {
        var logData = new List<string>();
        try
        {
            await ExecuteNormalFlow(siteDbGroupId, voidingQueues, logData);
        }
        catch (Exception e)
        {
            await loggingService.InfoAppendNewLineAsync(logData);
            await loggingService.ErrorAsync($"{nameof(VoidService)} Error", e);
        }
    }

    private async Task ExecuteNormalFlow(int siteDbGroupId, IList<TicketVoidingQueueModel> voidingQueues, IList<string> logData)
    {
        logData.Add($"{nameof(VoidService)}[{siteDbGroupId}]: {JsonSerializer.Serialize(voidingQueues)}");
        var processVoidQueuesResult = await processVoidQueuesStage.Execute(siteDbGroupId, voidingQueues);
        logData.Add($"{nameof(processVoidQueuesResult)}: {JsonSerializer.Serialize(processVoidQueuesResult)}");

        if (processVoidQueuesResult.All(x => x.ErrCode is ErrorCodeVoidQueue.NoNeedInsertVoidBetAutomation))
        {
            await requestDetailService.UpdateTicketVoidingHistory(voidingQueues[0].VoidingHistoryId, ExecuteStatus.Done);
            await loggingService.InfoAppendNewLineAsync(logData);
            return;
        }

        if (processVoidQueuesResult.Any(x => x.StatusId is ExecuteStatus.Failed or ExecuteStatus.New))
        {
            var firstVoidingQueue = voidingQueues[0];
            await requestDetailService.UpdateTicketVoidingHistory(firstVoidingQueue.VoidingHistoryId, ExecuteStatus.Failed);
            await notificationService.NotifyVoidFailedToUser(firstVoidingQueue.RequestId, processVoidQueuesResult);
            await notificationService.NotifyRequestFailedToAllUser(firstVoidingQueue.RequestId);
            await loggingService.InfoAppendNewLineAsync(logData);
            return;
        }

        if (processVoidQueuesResult.Any(x => x.StatusId == ExecuteStatus.Running))
        {
            await loggingService.InfoAppendNewLineAsync(logData);
            return;
        }

        await insertAfterVoidDataStage.Execute(processVoidQueuesResult, logData);

        logData.Add($"ServerTime: {DateTimeOffset.Now:MM/dd/yyyy HH:mm:ss.fff}");
        await loggingService.InfoAppendNewLineAsync(logData);
    }
}