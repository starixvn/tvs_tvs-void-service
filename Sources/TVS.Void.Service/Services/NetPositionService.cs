using System;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Criteria;

namespace TVS.Void.Service.Services;

public interface NetPositionService
{
    Task<bool> UpdateNetPosition(int siteDbGroupId, string transIds, string action);

    Task RollBackNetPosition(int siteDbGroupId, string transIds);
}

public class NetPositionServiceImp(LoggingService loggingService, IDynamicRepository repository)
    : NetPositionService
{
    public async Task<bool> UpdateNetPosition(int siteDbGroupId, string transIds, string action)
    {
        if (string.IsNullOrWhiteSpace(transIds))
        {
            return true;
        }

        try
        {
            await UpdateNetPositionTasks(siteDbGroupId, transIds, action);
            return true;
        }
        catch (Exception ex)
        {
            await loggingService.ErrorAsync(ex);
            return false;
        }
    }

    private async Task UpdateNetPositionTasks(int siteDbGroupId, string transIds, string action)
    {
        var command = new TicketVoidingNetPositionCriteria
        {
            StrTransIds = transIds,
            Action = action,
            SiteDbGroupId = siteDbGroupId
        };

        await repository.ExecuteAsync(command);
    }

    public async Task RollBackNetPosition(int siteDbGroupId, string transIds)
    {
        if (!string.IsNullOrWhiteSpace(transIds))
        {
            var command = new TicketVoidingNetPositionCriteria
            {
                StrTransIds = transIds,
                Action = NetPositionConstant.ActionRecover,
                SiteDbGroupId = siteDbGroupId
            };

            await repository.ExecuteAsync(command);
        }
    }
}