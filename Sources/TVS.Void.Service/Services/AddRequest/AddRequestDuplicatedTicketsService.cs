using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.AddRequest;

public interface AddRequestDuplicatedTicketsService
{
    Task Execute();
}

public class AddRequestDuplicatedTicketsServiceImp(
    GetDuplicatedTicketsStage getDuplicatedTicketsStage,
    CheckDuplicatedTicketsStage checkDuplicatedTicketsStage,
    MarkTicketsStage markTicketsStage,
    AddRequestStage addRequestStage,
    InsertAutoVoidRequestQueueStage insertAutoVoidRequestQueueStage,
    LoggingService loggingService)
    : AddRequestDuplicatedTicketsService
{
    public async Task Execute()
    {
        var logData = new List<string>();
        try
        {
            await ExecuteNormalFlow(logData);
        }
        catch (Exception e)
        {
            await loggingService.InfoAppendNewLineAsync(logData);
            await loggingService.ErrorAsync($"{nameof(AddRequestDuplicatedTicketsService)} Error", e);
        }
    }

    private async Task ExecuteNormalFlow(IList<string> logData)
    {
        var tickets = await getDuplicatedTicketsStage.Execute();
        if (string.IsNullOrWhiteSpace(tickets?.TransInfoJson))
        {
            return;
        }

        var validations = await checkDuplicatedTicketsStage.Execute(tickets.TransInfoJson);
        if (!validations.Any())
        {
            return;
        }

        await ExecuteWhenHaveData(validations, logData);
    }

    private async Task ExecuteWhenHaveData(IEnumerable<CheckDuplicatedTicketsResult> validations, IList<string> logData)
    {
        logData.Add($"AddRequest: {JsonSerializer.Serialize(validations)}");
        foreach (var item in validations)
        {
            logData.Add("---");
            var isMarkedSuccessful = await markTicketsStage.Execute(item.MarkTransIds, logData);
            if (!isMarkedSuccessful)
            {
                continue;
            }

            var requestId = await addRequestStage.Execute(item, logData);
            if (requestId <= 0)
            {
                continue;
            }

            await insertAutoVoidRequestQueueStage.Execute(requestId, item, logData);
        }

        logData.Add($"ServerTime: {DateTimeOffset.Now:MM/dd/yyyy HH:mm:ss.fff}");
        await loggingService.InfoAppendNewLineAsync(logData);
    }
}