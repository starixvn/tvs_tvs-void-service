using System;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Criteria;

namespace TVS.Void.Service.Services;

public interface NetPositionService
{
    Task<bool> UpdateNetPosition(string transIds, string action);

    Task RollBackNetPosition(string transIds);
}

public class NetPositionServiceImp : NetPositionService
{
    private readonly IDynamicRepository repository;

    public NetPositionServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<bool> UpdateNetPosition(string transIds, string action)
    {
        if (string.IsNullOrEmpty(transIds))
        {
            return true;
        }

        return await IsNetPosition(transIds, action);
    }

    private async Task<bool> IsNetPosition(string transIds, string action)
    {
        try
        {
            var command = new TicketVoidingNetPositionCriteria
            {
                StrTransIds = transIds,
                Action = action
            };

            await repository.ExecuteAsync(command);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task RollBackNetPosition(string transIds)
    {
        if (!string.IsNullOrWhiteSpace(transIds))
        {
            var command = new TicketVoidingNetPositionCriteria
            {
                StrTransIds = transIds,
                Action = NetPositionConstant.ActionRecover
            };

            await repository.ExecuteAsync(command);
        }
    }
}