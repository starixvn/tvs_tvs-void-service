using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Extensions;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services
{
    public interface RequestDetailService
    {
        Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>,
            IEnumerable<RequestDetailModel>)> CheckTicketsAllowToVoid(int siteDbGroupId, AutoVoidRequestQueueModel autoVoidRequestQueue);

        Task<IEnumerable<VoidServiceModel>> InsertVoidServiceData(string broadcastSessionId, int siteDbGroupId,
            IEnumerable<RequestDetailModel> requests);

        Task UpdateVoidService(int siteDbGroupId, int requestId, int voidingHistoryId, int statusId);
    }

    public class RequestDetailServiceImp : RequestDetailService
    {
        private readonly IDynamicRepository repository;

        public RequestDetailServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        private async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsBySiteDbGroupId(int siteDbGroupId, AutoVoidRequestQueueModel autoVoidRequestQueue)
        {
            var criteria = new GetRequestDetailTicketsCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                RequestId = autoVoidRequestQueue.RequestId,
                MatchId = autoVoidRequestQueue.MatchId,
                Action = autoVoidRequestQueue.Action
            };

            var (singleTickets, parlayTickets) =
                await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel>(criteria);
            return (singleTickets, parlayTickets);
        }

        public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailModel>)>
            CheckTicketsAllowToVoid(int siteDbGroupId, AutoVoidRequestQueueModel autoVoidRequestQueue)
        {
            var (singleTickets, parlayTickets) = await GetTicketsBySiteDbGroupId(siteDbGroupId, autoVoidRequestQueue);
            if ((singleTickets == null || !singleTickets.Any()) && (parlayTickets == null || !parlayTickets.Any()))
            {
                return (null, null, null);
            }
            var criteria = new CheckTicketsAllowToVoidCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                SingleTicketsXml = singleTickets.ToTicketsXml(),
                ParlayTicketsXml = parlayTickets.ToTicketsXml()
            };

            var (validSingleTickets, validParlayTickets, validRequests) =
                await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailModel>(criteria);
            return (validSingleTickets, validParlayTickets, validRequests);
        }

        public async Task<IEnumerable<VoidServiceModel>> InsertVoidServiceData(string broadcastSessionId, int siteDbGroupId, IEnumerable<RequestDetailModel> requests)
        {
            var criteria = new InsertVoidServiceDataCriteria
            {
                BroadcastSessionId = broadcastSessionId,
                SiteDbGroupId = siteDbGroupId,
                IsAutoVoid = true,
                RequestInfoXml = requests.ToRequestInfoXml()
            };

            var voidServiceData = await repository.FetchAsync<VoidServiceModel>(criteria);
            return voidServiceData;
        }

        public async Task UpdateVoidService(int siteDbGroupId, int requestId, int voidingHistoryId, int statusId)
        {
            var criteria = new UpdateVoidServiceCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                RequestId = requestId,
                VoidingHistoryId = voidingHistoryId,
                StatusId = statusId
            };

            await repository.ExecuteAsync(criteria);
        }
    }
}