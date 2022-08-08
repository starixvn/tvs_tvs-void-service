using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface AutoVoidRequestQueueService
    {
        Task<AutoVoidRequestQueueModel> GetValidQueue(int siteDbGroupId);

        Task UpdateQueue(int siteDbGroupId, int queueId, int? statusId, string message, int? createdDateMinuteOffset);

        Task InsertQueue(int siteDbGroupId, string broadcastSessionId, int requestId, int matchId, int statusId, string action);
    }

    public class AutoVoidRequestQueueServiceImp : AutoVoidRequestQueueService
    {
        private readonly IDynamicRepository repository;

        public AutoVoidRequestQueueServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<AutoVoidRequestQueueModel> GetValidQueue(int siteDbGroupId)
        {
            var criteria = new GetAutoVoidRequestQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId
            };

            var result = await repository.GetAsync<AutoVoidRequestQueueModel>(criteria);
            return result;
        }

        public async Task UpdateQueue(int siteDbGroupId, int queueId, int? statusId, string message, int? createdDateMinuteOffset)
        {
            var criteria = new UpdateAutoVoidRequestQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                QueueId = queueId,
                StatusId = statusId,
                Message = message,
                CreatedDateMinuteOffset = createdDateMinuteOffset
            };

            await repository.ExecuteAsync(criteria);
        }

        public async Task InsertQueue(int siteDbGroupId, string broadcastSessionId, int requestId, int matchId, int statusId, string action)
        {
            var criteria = new InsertAutoVoidRequestQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                BroadcastSessionId = broadcastSessionId,
                RequestId = requestId,
                MatchId = matchId,
                StatusId = statusId,
                Action = action
            };

            await repository.ExecuteAsync(criteria);
        }
    }
}