using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Extensions;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest
{
    public interface CheckTicketAllowToVoidStage
    {
        Task<CheckStatusTicketOutput> Execute(int siteDbGroupId, string queueAction,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets, IList<string> logData);
    }

    public class CheckTicketAllowToVoidStageImp : CheckTicketAllowToVoidStage
    {
        private readonly IDynamicRepository repository;

        public CheckTicketAllowToVoidStageImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<CheckStatusTicketOutput> Execute(int siteDbGroupId, string queueAction,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets, IList<string> logData)
        {
            var criteria = new CheckTicketsAllowToVoidCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                Action = queueAction,
                SingleTicketsXml = singleTickets.ToTicketsXml(),
                ParlayTicketsXml = parlayTickets.ToTicketsXml()
            };

            var mappingTypes = new[]
            {
                typeof (RequestDetailTicketModel),
                typeof (RequestDetailTicketModel),
                typeof(RequestDetailTicketModel),
                typeof(RequestDetailTicketModel),
            };

            var objectsArray = await repository.FetchMultipleAsync(criteria, mappingTypes);

            var outputData = new CheckStatusTicketOutput
            {
                ValidVoidSingleTickets = objectsArray[0].Cast<RequestDetailTicketModel>(),
                ValidVoidParlayTickets = objectsArray[1].Cast<RequestDetailTicketModel>(),
                SingleTicketsResettle = objectsArray[2].Cast<RequestDetailTicketModel>(),
                ParlayTicketsResettle = objectsArray[3].Cast<RequestDetailTicketModel>(),
            };

            logData.Add($"{nameof(CheckTicketAllowToVoidStage)} - VVSingle: {outputData.ValidVoidSingleTickets.Count()} - VVParlay: {outputData.ValidVoidParlayTickets.Count()} " +
                               $"- SingleResettle: {outputData.SingleTicketsResettle.Count()} - ParlayResettle: {outputData.ParlayTicketsResettle.Count()}");

            return outputData;
        }
    }
}