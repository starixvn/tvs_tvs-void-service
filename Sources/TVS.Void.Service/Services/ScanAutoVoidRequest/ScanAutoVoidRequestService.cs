using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface ScanAutoVoidRequestService
{
    Task Execute(int siteDbGroupId);
}

public class ScanAutoVoidRequestServiceImp : ScanAutoVoidRequestService
{
    private readonly AutoVoidWinningProcessStage autoVoidWinningProcessStage;
    private readonly AutoVoidInstantProcessStage autoVoidInstantProcessStage;
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly LoggingService loggingService;

    public ScanAutoVoidRequestServiceImp(AutoVoidWinningProcessStage autoVoidWinningProcessStage, AutoVoidInstantProcessStage autoVoidInstantProcessStage,
        AutoVoidRequestQueueService autoVoidRequestQueueService, LoggingService loggingService)
    {
        this.autoVoidWinningProcessStage = autoVoidWinningProcessStage;
        this.autoVoidInstantProcessStage = autoVoidInstantProcessStage;

        this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        this.loggingService = loggingService;
    }

    public async Task Execute(int siteDbGroupId)
    {
        var queue = await autoVoidRequestQueueService.GetQueue(siteDbGroupId);
        if (queue == null)
        {
            return;
        }

        await ExecuteWhenHaveData(siteDbGroupId, queue);
    }

    private async Task ExecuteWhenHaveData(int siteDbGroupId, AutoVoidRequestQueueModel queue)
    {
        var logData = new List<string>();
        try
        {
            logData.Add($"Scan[{siteDbGroupId}]: {JsonSerializer.Serialize(queue)}");
            if (IsVoidInstantQueue(queue.Action))
            {
                await autoVoidInstantProcessStage.Execute(siteDbGroupId, queue, logData);
            }
            else
            {
                await autoVoidWinningProcessStage.Execute(siteDbGroupId, queue, logData);
            }
        }
        catch (Exception e)
        {
            await loggingService.InfoAppendNewLineAsync(logData);
            await loggingService.ErrorAsync($"{nameof(ScanAutoVoidRequestService)} Error", e);
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Failed, null);
        }
    }

    private static bool IsVoidInstantQueue(string action)
    {
        return action.Equals(AutoVoidConstant.QueueActionVoidInstant, StringComparison.OrdinalIgnoreCase)
               || action.Equals(AutoVoidConstant.QueueActionVoidInstant5m, StringComparison.OrdinalIgnoreCase)
               || action.Equals(AutoVoidConstant.QueueActionVoidInstant10m, StringComparison.OrdinalIgnoreCase)
               || action.Equals(AutoVoidConstant.QueueActionVoidInstantNoPolicy, StringComparison.OrdinalIgnoreCase);
    }
}