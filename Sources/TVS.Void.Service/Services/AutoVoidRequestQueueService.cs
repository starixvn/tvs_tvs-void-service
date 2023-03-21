using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services;

public interface AutoVoidRequestQueueService
{
    Task<AutoVoidRequestQueueModel> GetQueue(int siteDbGroupId);

    Task UpdateQueue(int siteDbGroupId, int queueId, int? statusId, string errorMessage);

    Task InsertQueue(int siteDbGroupId, string broadcastSessionId, string requestIds, int statusId, string action, int? matchId);
}

public class AutoVoidRequestQueueServiceImp : AutoVoidRequestQueueService
{
    private readonly IDynamicRepository repository;

    public AutoVoidRequestQueueServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<AutoVoidRequestQueueModel> GetQueue(int siteDbGroupId)
    {
        var criteria = new GetAutoVoidRequestQueueCriteria
        {
            SiteDbGroupId = siteDbGroupId,
        };

        var result = await repository.GetAsync<AutoVoidRequestQueueModel>(criteria);
        if (result != null)
        {
            result.SiteDbGroupId = siteDbGroupId;
        }

        return result;
    }

    public async Task UpdateQueue(int siteDbGroupId, int queueId, int? statusId, string errorMessage)
    {
        var criteria = new UpdateAutoVoidRequestQueueCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            QueueId = queueId,
            StatusId = statusId,
            ErrMsg = errorMessage
        };

        await repository.ExecuteAsync(criteria);
    }

    public async Task InsertQueue(int siteDbGroupId, string broadcastSessionId, string requestIds, int statusId, string action, int? matchId)
    {
        var criteria = new InsertAutoVoidRequestQueueCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            BroadcastSessionId = broadcastSessionId,
            RequestIds = requestIds,
            StatusId = statusId,
            Action = action,
            MatchId = matchId
        };

        await repository.ExecuteAsync(criteria);
    }
}