using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface CheckTicketValidationStage
{
    Task<CheckTicketValidationOutput> ExecuteForVoid(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets,
        IEnumerable<RequestDetailTicketModel> parlayTickets, IEnumerable<RequestDetailTicketModel> cricketTickets, IList<string> logData);

    Task<CheckTicketValidationOutput> ExecuteForRecover(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets,
        IEnumerable<RequestDetailTicketModel> parlayTickets, IEnumerable<RequestDetailTicketModel> cricketTickets, IList<string> logData);
}

public class CheckTicketValidationStageImp : CheckTicketValidationStage
{
    private readonly IDynamicRepository repository;

    public CheckTicketValidationStageImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<CheckTicketValidationOutput> ExecuteForVoid(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets,
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
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel)
        };

        var objectsArray = await repository.FetchMultipleAsync(criteria, mappingTypes);
        var output = new CheckTicketValidationOutput
        {
            VoidSingleTickets = objectsArray[0].Cast<RequestDetailTicketModel>(),
            SingleTicketsResettle = objectsArray[1].Cast<RequestDetailTicketModel>(),
            VoidParlayTickets = objectsArray[2].Cast<RequestDetailTicketModel>(),
            ParlayTicketsResettle = objectsArray[3].Cast<RequestDetailTicketModel>(),
            VoidCricketTickets = objectsArray[4].Cast<RequestDetailTicketModel>(),
            CricketTicketsResettle = objectsArray[5].Cast<RequestDetailTicketModel>(),
            //ValidCancelCOTickets = objectsArray[6].Cast<RequestDetailTicketModel>() (Only use for manual void issue type)
            VoidSingleTickets14 = objectsArray[7].Cast<RequestDetailTicketModel>(),
            VoidParlayTickets14 = objectsArray[8].Cast<RequestDetailTicketModel>()
        };

        logData.Add($"{nameof(CheckTicketValidationStage)}.{nameof(ExecuteForVoid)} - vSingle: {output.VoidSingleTickets.Count()} - vParlay: {output.VoidParlayTickets.Count()}"
                        + $" - SingleResettle: {output.SingleTicketsResettle.Count()} - ParlayResettle: {output.ParlayTicketsResettle.Count()}"
                        + $" - vCricketTickets: {output.VoidCricketTickets.Count()} - CricketTicketsResettle: {output.CricketTicketsResettle.Count()}"
                        + $" - vSingle14: {output.VoidSingleTickets14.Count()} - vParlay14: {output.VoidParlayTickets14.Count()}");

        return output;
    }

    public async Task<CheckTicketValidationOutput> ExecuteForRecover(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets,
        IEnumerable<RequestDetailTicketModel> cricketTickets, IList<string> logData)
    {
        var criteria = new CheckTicketsAllowToRecoverCriteria
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
         };

        var objectsArray = await repository.FetchMultipleAsync(criteria, mappingTypes);
        var output = new CheckTicketValidationOutput
        {
            RecoverSingleTickets = objectsArray[0].Cast<RequestDetailTicketModel>(),
            RecoverParlayTickets = objectsArray[1].Cast<RequestDetailTicketModel>(),
            RecoverCricketTickets = objectsArray[2].Cast<RequestDetailTicketModel>(),
            RecoverSingleTickets14 = objectsArray[3].Cast<RequestDetailTicketModel>(),
            RecoverParlayTickets14 = objectsArray[4].Cast<RequestDetailTicketModel>()
        };

        logData.Add($"{nameof(CheckTicketValidationStage)}.{nameof(ExecuteForRecover)} - rSingle: {output.RecoverSingleTickets.Count()} - rParlay: {output.RecoverParlayTickets.Count()}"
                    + $" - rCricket: {output.RecoverCricketTickets.Count()}"
                    + $" - rSingle14: {output.RecoverSingleTickets14.Count()} - rParlay14: {output.RecoverParlayTickets14.Count()}");

        return output;
    }
}