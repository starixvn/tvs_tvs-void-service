using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria.MultiSiteDb;

namespace TVS.Void.Service.Services.AddRequest;

public interface MarkTicketsStage
{
    Task<bool> Execute(string markTransIds, IList<string> logData);
}

public class MarkTicketsStageImp : MarkTicketsStage
{
    private readonly IDynamicRepository repository;
    private readonly LoggingService loggingService;

    public MarkTicketsStageImp(IDynamicRepository repository, LoggingService loggingService)
    {
        this.repository = repository;
        this.loggingService = loggingService;
    }

    public async Task<bool> Execute(string markTransIds, IList<string> logData)
    {
        try
        {
            var markResult = await MarkTicketsInAllSiteDb(markTransIds);
            logData.Add($"{nameof(MarkTicketsStage)}: {JsonSerializer.Serialize(markResult)}");
            return markResult.All(x => x.IsSuccessful);
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync(e);
            return false;
        }
    }

    private async Task<IEnumerable<Result>> MarkTicketsInAllSiteDb(string markTransIds)
    {
        var markTicketsCommand = new MarkTicketsCriteria
        {
            TransIds = markTransIds,
            Status = MarkTypeConstant.ExecuteMarkTickets,
            IssueTypeId = IssueTypeConstant.DuplicateTickets,
            UserId = AutoVoidConstant.ActorId,
            Ip = AutoVoidConstant.IPAddress,
            LogActionId = LogActionIdConstant.MarkTicketAddRequestTvs
        };

        var tasks = SiteDbGroupConstant.ServerList.Select(x =>
        {
            markTicketsCommand.SiteDbGroupId = x.Id;
            return repository.FetchAsync<Result>(markTicketsCommand);
        });

        var tasksResults = await Task.WhenAll(tasks);
        var resultsInAllServer = tasksResults.SelectMany(data => data);
        return resultsInAllServer;
    }
}