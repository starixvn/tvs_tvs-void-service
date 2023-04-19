using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface CheckTicketAllowToVoidStage
{
    Task<CheckStatusTicketOutput> Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets,
        IEnumerable<RequestDetailTicketModel> parlayTickets, IEnumerable<RequestDetailTicketModel> cricketTickets, IList<string> logData);
}

public class CheckTicketAllowToVoidStageImp : CheckTicketAllowToVoidStage
{
    private readonly IDynamicRepository repository;

    public CheckTicketAllowToVoidStageImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<CheckStatusTicketOutput> Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets,
        IEnumerable<RequestDetailTicketModel> parlayTickets, IEnumerable<RequestDetailTicketModel> cricketTickets, IList<string> logData)
    {
        var criteria = new CheckTicketsAllowToVoidCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            SingleTicketsJson = JsonSerializer.Serialize(singleTickets),
            ParlayTicketsJson = JsonSerializer.Serialize(parlayTickets),
            CricketTicketsJson = JsonSerializer.Serialize(cricketTickets)
        };

        var mappingTypes = new[]
        {
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel)
        };

        var objectsArray = await repository.FetchMultipleAsync(criteria, mappingTypes);

        var outputData = new CheckStatusTicketOutput
        {
            ValidVoidSingleTickets = objectsArray[0].Cast<RequestDetailTicketModel>(),
            SingleTicketsResettle = objectsArray[1].Cast<RequestDetailTicketModel>(),
            ValidVoidParlayTickets = objectsArray[2].Cast<RequestDetailTicketModel>(),
            ParlayTicketsResettle = objectsArray[3].Cast<RequestDetailTicketModel>(),
            ValidVoidCricketTickets = objectsArray[4].Cast<RequestDetailTicketModel>(),
            CricketTicketsResettle = objectsArray[5].Cast<RequestDetailTicketModel>(),
        };

        logData.Add($"{nameof(CheckTicketAllowToVoidStage)} - VVSingle: {outputData.ValidVoidSingleTickets.Count()} - VVParlay: {outputData.ValidVoidParlayTickets.Count()} - "
                    + $"SingleResettle: {outputData.SingleTicketsResettle.Count()} - ParlayResettle: {outputData.ParlayTicketsResettle.Count()}"
                    + $"ValidVoidCricketTickets: {outputData.ValidVoidCricketTickets.Count()} - CricketTicketsResettle: {outputData.CricketTicketsResettle.Count()}");

        return outputData;
    }
}