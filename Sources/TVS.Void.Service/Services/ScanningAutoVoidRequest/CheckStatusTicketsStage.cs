using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Extensions;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface CheckStatusTicketsStage
    {
        Task<CheckStatusTicketOutput> Execute(int siteDbGroupId, string queueAction,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets, StringBuilder logData);
    }

    public class CheckStatusTicketsStageImp : CheckStatusTicketsStage
    {
        private readonly IDynamicRepository repository;

        public CheckStatusTicketsStageImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task<CheckStatusTicketOutput> Execute(int siteDbGroupId, string queueAction,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets, StringBuilder logData)
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
                typeof(RequestModel)
            };

            var objectsArray = await repository.FetchMultipleAsync(criteria, mappingTypes);

            var outputData = new CheckStatusTicketOutput
            {
                ValidVoidSingleTickets = objectsArray[0].Cast<RequestDetailTicketModel>(),
                ValidVoidParlayTickets = objectsArray[1].Cast<RequestDetailTicketModel>(),
                SingleTicketsResettle = objectsArray[2].Cast<RequestDetailTicketModel>(),
                ParlayTicketsResettle = objectsArray[3].Cast<RequestDetailTicketModel>(),
                MixParlayRequestsNoStatusWLButAllChildMatchesCompleted = objectsArray[4].Cast<RequestModel>()
            };

            logData.AppendLine($"{nameof(CheckStatusTicketsStage)}: {JsonSerializer.Serialize(outputData)}");

            return outputData;
        }
    }
}