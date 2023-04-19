using System;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Criteria;

namespace TVS.Void.Service.Services;

public interface NetPositionService
{
    Task<bool> UpdateNetPosition(string transIds, string action, int userId);

    Task RollBackNetPosition(string transIds, string action, string message, int userId);
}

public class NetPositionServiceImp : NetPositionService
{
    private readonly NetPositionHttpClientService netPositionHttpClientService;
    private readonly IDynamicRepository repository;

    public NetPositionServiceImp(NetPositionHttpClientService netPositionHttpClientService, IDynamicRepository repository)
    {
        this.netPositionHttpClientService = netPositionHttpClientService;
        this.repository = repository;
    }

    public async Task<bool> UpdateNetPosition(string transIds, string action, int userId)
    {
        if (!string.IsNullOrEmpty(transIds))
        {
            var param = new UpdateNetPositionLogCommand
            {
                AdminId = userId,
                ServiceType = ServiceApiType.Cricket,
                TransIds = transIds,
                Action = action
            };

            return await ExecuteNetPosition(param);
        }

        return true;
    }

    private async Task<bool> ExecuteNetPosition(UpdateNetPositionLogCommand command)
    {
        command.StartTime = DateTime.Now;
        var result = await netPositionHttpClientService.PostRequest(command.TransIds, command.Action);
        command.EndTime = DateTime.Now;

        if (result.IsSuccessful)
        {
            command.ServiceStatus = ServiceApiStatus.Finished;
        }
        else
        {
            command.ServiceStatus = ServiceApiStatus.Failed;
            command.Reason = result.ErrorMsg;
        }

        await repository.ExecuteAsync(command);

        return result.IsSuccessful;
    }

    public async Task RollBackNetPosition(string transIds, string action, string message, int userId)
    {
        if (!string.IsNullOrWhiteSpace(transIds))
        {
            var command = new RollBackNetPositionCommand
            {
                CricketIds = transIds,
                Action = action,
                AdminId = userId,
                Reason = message
            };
            await repository.ExecuteAsync(command);
        }
    }
}