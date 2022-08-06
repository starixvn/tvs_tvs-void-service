using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface AutoVoidRequestQueueService
    {
        Task<AutoVoidRequestQueueModel> GetValidQueue(int siteDbGroupId);

        Task UpdateQueueStatus(int siteDbGroupId, int queueId, int statusId, string message);
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

        public async Task UpdateQueueStatus(int siteDbGroupId, int queueId, int statusId, string message)
        {
            var criteria = new UpdateAutoVoidRequestQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                QueueId = queueId,
                StatusId = statusId,
                Message = message
            };

            await repository.ExecuteAsync(criteria);
        }
    }
}