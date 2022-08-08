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
        Task GetQueues(int siteDbGroupId);

        Task InsertQueue(int siteDbGroupId, int voidingHistoryId, int requestId, int issueTypeId,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);
    }

    public class TicketVoidingQueueServiceImp : TicketVoidingQueueService
    {
        private readonly IDynamicRepository repository;

        public TicketVoidingQueueServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task GetQueues(int siteDbGroupId)
        {
            var criteria = new GetTicketVoidingQueueCriteria
            {
                SiteDbGroupId = siteDbGroupId
            };
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
    }
}