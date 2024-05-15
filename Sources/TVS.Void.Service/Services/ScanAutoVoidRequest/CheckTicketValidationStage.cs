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

        var results = (await repository.FetchAsync<RequestDetailTicketModel>(criteria)).ToList();
        var voidSingleTickets = results.Where(x => x.ValidationType == 1);
        var singleTicketsResettle = results.Where(x => x.ValidationType == 2);
        var voidParlayTickets = results.Where(x => x.ValidationType == 3);
        var parlayTicketsResettle = results.Where(x => x.ValidationType == 4);
        var voidCricketTickets = results.Where(x => x.ValidationType == 5);
        var cricketTicketsResettle = results.Where(x => x.ValidationType == 6);
        //// var cancelCOTickets = results.Where(x => x.ValidationType == 7); (Only use for manual void issue type)
        var voidSingleTickets14 = results.Where(x => x.ValidationType == 8);
        var voidParlayTickets14 = results.Where(x => x.ValidationType == 9);

        var output = new CheckTicketValidationOutput
        {
            VoidSingleTickets = voidSingleTickets,
            SingleTicketsResettle = singleTicketsResettle,
            VoidParlayTickets = voidParlayTickets,
            ParlayTicketsResettle = parlayTicketsResettle,
            VoidCricketTickets = voidCricketTickets,
            CricketTicketsResettle = cricketTicketsResettle,
            //ValidCancelCOTickets = objectsArray[6].Cast<RequestDetailTicketModel>() (Only use for manual void issue type)
            VoidSingleTickets14 = voidSingleTickets14,
            VoidParlayTickets14 = voidParlayTickets14
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

        var (recoverSingleTickets, recoverParlayTickets, recoverCricketTickets, recoverSingleTickets14, recoverParlayTickets14) =
            await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>(criteria);
        var output = new CheckTicketValidationOutput
        {
            RecoverSingleTickets = recoverSingleTickets,
            RecoverParlayTickets = recoverParlayTickets,
            RecoverCricketTickets = recoverCricketTickets,
            RecoverSingleTickets14 = recoverSingleTickets14,
            RecoverParlayTickets14 = recoverParlayTickets14
        };

        logData.Add($"{nameof(CheckTicketValidationStage)}.{nameof(ExecuteForRecover)} - rSingle: {output.RecoverSingleTickets.Count()} - rParlay: {output.RecoverParlayTickets.Count()}"
                    + $" - rCricket: {output.RecoverCricketTickets.Count()}"
                    + $" - rSingle14: {output.RecoverSingleTickets14.Count()} - rParlay14: {output.RecoverParlayTickets14.Count()}");

        return output;
    }
}