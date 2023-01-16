using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface RequestTicketService
    {
        Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsByRequestsAndSiteDbGroupId(
            int siteDbGroupId, string requestIds);

        Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketInAllRequests();

        Task<IEnumerable<RequestModel>> CheckStatusTicketForInsertAutoVoidQueue(int siteDbGroupId,
                    IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);

        Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);
    }

    public class RequestTicketServiceImp : RequestTicketService
    {
        private readonly IDynamicRepository repository;

        public RequestTicketServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsByRequestsAndSiteDbGroupId(int siteDbGroupId, string requestIds)
        {
            return await GetTicketsInRequests(siteDbGroupId, requestIds);
        }

        public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketInAllRequests()
        {
            return await GetTicketsInRequests(null, null);
        }

        private async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsInRequests(int? siteDbGroupId, string requestIds)
        {
            var criteria = new GetRequestTicketsCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                RequestIds = requestIds,
            };

            var (singleTickets, parlayTickets) =
                await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel>(criteria);
            return (singleTickets, parlayTickets);
        }

        public async Task<IEnumerable<RequestModel>> CheckStatusTicketForInsertAutoVoidQueue(int siteDbGroupId,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            var criteria = new CheckStatusTicketCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                SingleTicketsJson = JsonSerializer.Serialize(singleTickets),
                ParlayTicketsJson = JsonSerializer.Serialize(parlayTickets)
            };

            return await repository.FetchAsync<RequestModel>(criteria);
        }

        public async Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            var criteria = new UpdateIsProcessedForRequestTicketsCriteria
            {
                SingleTicketsJson = JsonSerializer.Serialize(singleTickets),
                ParlayTicketsJson = JsonSerializer.Serialize(parlayTickets)
            };

            await repository.ExecuteAsync(criteria);
        }
    }
}