using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Extensions;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface TicketVoidingQueueService
    {
        Task<IEnumerable<TicketVoidingQueueModel>> GetQueues(int siteDbGroupId);

        Task InsertQueue(int siteDbGroupId, int voidingHistoryId, int requestId, int issueTypeId,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);

        Task<ProcessVoidQueueResult> ProcessVoidQueue(int siteDbGroupId, int queueId);
    }

    public class TicketVoidingQueueServiceImp : TicketVoidingQueueService
    {
        private readonly IDynamicRepository repository;

        public TicketVoidingQueueServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IEnumerable<TicketVoidingQueueModel>> GetQueues(int siteDbGroupId)
        {
            var criteria = new GetTicketVoidingQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId
            };

            var result = await repository.FetchAsync<TicketVoidingQueueModel>(criteria);
            return result;
        }

        public async Task InsertQueue(int siteDbGroupId, int voidingHistoryId, int requestId, int issueTypeId,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            var criteria = new InsertTicketVoidingQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                RequestId = requestId,
                VoidingHistoryId = voidingHistoryId,
                IssueTypeId = issueTypeId,
                SingleTicketsXml = singleTickets.ToTicketsXml(),
                ParlayTicketsXml = parlayTickets.ToTicketsXml(),
                UserId = AutoVoidConstant.ActorId,
                IP = AutoVoidConstant.IPAddress,
            };
            await repository.ExecuteAsync(criteria);
        }

        public async Task<ProcessVoidQueueResult> ProcessVoidQueue(int siteDbGroupId, int queueId)
        {
            var criteria = new ProcessVoidQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                QueueId = queueId
            };

            var result = await repository.GetAsync<ProcessVoidQueueResult>(criteria);
            return result;
        }
    }
}