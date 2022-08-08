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
        Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsBySiteDbGroupId(
            int siteDbGroupId, AutoVoidRequestQueueModel autoVoidRequestQueue);

        Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>,
                IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailModel>)>
            CheckStatusTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);

        Task<IEnumerable<VoidServiceModel>> InsertVoidServiceData(string broadcastSessionId, int siteDbGroupId,
            IEnumerable<RequestDetailModel> requests);

        Task UpdateVoidService(int siteDbGroupId, int requestId, int voidingHistoryId, int statusId);

        Task ReCalculateTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);
    }

    public class RequestDetailServiceImp : RequestDetailService
    {
        private readonly IDynamicRepository repository;

        public RequestDetailServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsBySiteDbGroupId(int siteDbGroupId, AutoVoidRequestQueueModel autoVoidRequestQueue)
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

        public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>,
                IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailModel>)>
            CheckStatusTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            var criteria = new CheckTicketsAllowToVoidCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                SingleTicketsXml = singleTickets.ToTicketsXml(),
                ParlayTicketsXml = parlayTickets.ToTicketsXml()
            };

            var mappingTypes = new[]
            {
                typeof (RequestDetailTicketModel),
                typeof (RequestDetailTicketModel),
                typeof(RequestDetailTicketModel),
                typeof(RequestDetailTicketModel),
                typeof(RequestDetailModel)
            };

            var objectsArray = await repository.FetchMultipleAsync(criteria, mappingTypes);
            var validVoidSingleTickets = objectsArray[0].Cast<RequestDetailTicketModel>();
            var validVoidParlayTickets = objectsArray[1].Cast<RequestDetailTicketModel>();
            var singleTicketsResettle = objectsArray[2].Cast<RequestDetailTicketModel>();
            var parlayTicketsResettle = objectsArray[3].Cast<RequestDetailTicketModel>();
            var mixParlayRequestsNoStatusWinLossButAllChildMatchesCompleted = objectsArray[4].Cast<RequestDetailModel>();

            return (validVoidSingleTickets, validVoidParlayTickets, singleTicketsResettle, parlayTicketsResettle, mixParlayRequestsNoStatusWinLossButAllChildMatchesCompleted);
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

        public async Task ReCalculateTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            var criteria = new ReCalculateTicketsCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                SingleTicketsXml = singleTickets.ToTicketsXml(),
                ParlayTicketsXml = parlayTickets.ToTicketsXml()
            };

            await repository.ExecuteAsync(criteria);
        }
    }
}