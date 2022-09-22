using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface AutoVoidRequestQueueService
    {
        Task<AutoVoidRequestQueueModel> GetQueue(int siteDbGroupId, string broadcastSessionId);

        Task UpdateQueue(int siteDbGroupId, int queueId, string broadcastSessionId, int? statusId, bool? isUpdateStartTime, string errorMessage);

        Task InsertQueue(int siteDbGroupId, string broadcastSessionId, int requestId, int matchId, int statusId, string action);
    }

    public class AutoVoidRequestQueueServiceImp : AutoVoidRequestQueueService
    {
        private readonly IDynamicRepository repository;

        public AutoVoidRequestQueueServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// If broadcastSessionId == null, only get valid queue
        /// </summary>
        /// <param name="siteDbGroupId"></param>
        /// <param name="broadcastSessionId"></param>
        /// <returns></returns>
        public async Task<AutoVoidRequestQueueModel> GetQueue(int siteDbGroupId, string broadcastSessionId)
        {
            var criteria = new GetAutoVoidRequestQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                BroadcastSessionId = broadcastSessionId
            };

            var result = await repository.GetAsync<AutoVoidRequestQueueModel>(criteria);
            if (result != null)
            {
                result.SiteDbGroupId = siteDbGroupId;
            }

            return result;
        }

        public async Task UpdateQueue(int siteDbGroupId, int queueId, string broadcastSessionId, int? statusId, bool? isUpdateStartTime, string errorMessage)
        {
            var criteria = new UpdateAutoVoidRequestQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                QueueId = queueId,
                BroadcastSessionId = broadcastSessionId,
                StatusId = statusId,
                IsUpdateStartTime = isUpdateStartTime,
                ErrMsg = errorMessage
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