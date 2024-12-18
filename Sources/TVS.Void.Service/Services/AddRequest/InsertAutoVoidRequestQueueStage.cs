using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.AddRequest;

public interface InsertAutoVoidRequestQueueStage
{
    Task Execute(int requestId, CheckDuplicatedTicketsResult validation, IList<string> logData);
}

public class InsertAutoVoidRequestQueueStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService)
    : InsertAutoVoidRequestQueueStage
{
    public async Task Execute(int requestId, CheckDuplicatedTicketsResult validation, IList<string> logData)
    {
        var guid = Guid.NewGuid().ToString();
        var listSiteDb = validation.SiteDBAffected.Split(',');
        var tasks = listSiteDb.Select(id =>
        {
            var siteDbGroupId = int.Parse(id);
            return autoVoidRequestQueueService.InsertQueue(siteDbGroupId, guid, requestId.ToString(),
                ExecuteStatus.New, AutoVoidConstant.QueueActionVoidInstant, null);
        });

        await Task.WhenAll(tasks);

        logData.Add($"{nameof(InsertAutoVoidRequestQueueStage)} - SessionId: {guid}");
    }
}