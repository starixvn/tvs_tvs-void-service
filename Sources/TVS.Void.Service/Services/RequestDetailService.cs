using System.Collections.Generic;
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
        Task<(IEnumerable<RequestDetailModel>, IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsBySiteDbGroupId(int siteDbGroupId, int matchId);

        Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> CheckTicketsAllowToVoid(int siteDbGroupId,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);
    }

    public class RequestDetailServiceImp : RequestDetailService
    {
        private readonly IDynamicRepository repository;

        public RequestDetailServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<(IEnumerable<RequestDetailModel>, IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketsBySiteDbGroupId(int siteDbGroupId, int matchId)
        {
            var criteria = new GetRequestDetailTicketsCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                MatchId = matchId
            };

            var (requests, singleTickets, parlayTickets) =
                await repository.FetchMultipleAsync<RequestDetailModel, RequestDetailTicketModel, RequestDetailTicketModel>(criteria);
            return (requests, singleTickets, parlayTickets);
        }

        public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> CheckTicketsAllowToVoid(int siteDbGroupId,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            var criteria = new CheckTicketsAllowToVoidCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                SingleTickets = singleTickets.ToTicketsXml(),
                ParlayTickets = parlayTickets.ToTicketsXml()
            };

            var (validSingleTickets, validParlayTickets) =
                await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel>(criteria);
            return (validSingleTickets, validParlayTickets);
        }
    }
}