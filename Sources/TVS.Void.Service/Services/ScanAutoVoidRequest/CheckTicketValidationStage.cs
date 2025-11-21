using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface CheckTicketValidationStage
{
    Task<CheckTicketValidationOutput> ExecuteForVoid(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets,
        IEnumerable<RequestDetailTicketModel> parlayTickets, IEnumerable<RequestDetailTicketModel> cricketTickets,
        IEnumerable<RequestDetailTicketModel> cancelCashOutTickets, IList<string> logData);

    Task<CheckTicketValidationOutput> ExecuteForRecover(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets,
        IEnumerable<RequestDetailTicketModel> parlayTickets, IEnumerable<RequestDetailTicketModel> cricketTickets, IList<string> logData);
}

public class CheckTicketValidationStageImp(IDynamicRepository repository) : CheckTicketValidationStage
{
    public async Task<CheckTicketValidationOutput> ExecuteForVoid(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets,
        IEnumerable<RequestDetailTicketModel> cricketTickets, IEnumerable<RequestDetailTicketModel> cancelCashOutTickets, IList<string> logData)
    {
        var criteria = new CheckTicketsAllowToVoidCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            SingleTicketsJson = JsonSerializer.Serialize(singleTickets),
            ParlayTicketsJson = JsonSerializer.Serialize(parlayTickets),
            CricketTicketsJson = JsonSerializer.Serialize(cricketTickets),
            CancelCashOutTicketsJson = JsonSerializer.Serialize(cancelCashOutTickets)
        };

        var results = (await repository.FetchAsync<RequestDetailTicketModel>(criteria)).ToList();
        var voidSingleTickets = results.Where(x => x.ValidationType == 1);
        var singleTicketsResettle = results.Where(x => x.ValidationType == 2);
        var voidParlayTickets = results.Where(x => x.ValidationType == 3);
        var parlayTicketsResettle = results.Where(x => x.ValidationType == 4);
        var voidCricketTickets = results.Where(x => x.ValidationType == 5);
        var cricketTicketsResettle = results.Where(x => x.ValidationType == 6);
        var outputCancelCashOutTickets = results.Where(x => x.ValidationType == 7); // user for auto void instant
        var voidSingleTickets14 = results.Where(x => x.ValidationType == 8);
        var voidParlayTickets14 = results.Where(x => x.ValidationType == 9);
        var hybridParlayMainTickets = results.Where(x => x.ValidationType == 10);
        var hybridParlayMainTicketsResettle = results.Where(x => x.ValidationType == 11);
        var hybridParlaySubTickets = results.Where(x => x.ValidationType == 12);
        var hybridParlayMainTickets14 = results.Where(x => x.ValidationType == 13);

        var output = new CheckTicketValidationOutput
        {
            VoidSingleTickets = voidSingleTickets,
            SingleTicketsResettle = singleTicketsResettle,
            VoidParlayTickets = voidParlayTickets,
            ParlayTicketsResettle = parlayTicketsResettle,
            VoidCricketTickets = voidCricketTickets,
            CricketTicketsResettle = cricketTicketsResettle,
            CancelCOTickets = outputCancelCashOutTickets,
            VoidSingleTickets14 = voidSingleTickets14,
            VoidParlayTickets14 = voidParlayTickets14,
            VoidHybridParlayMainTickets = hybridParlayMainTickets,
            VoidHybridParlaySubTickets = hybridParlayMainTicketsResettle,
            HybridParlayMainTicketsResettle = hybridParlaySubTickets,
            HybridParlayMainTickets14 = hybridParlayMainTickets14,
        };

        logData.Add($"{nameof(CheckTicketValidationStage)}.{nameof(ExecuteForVoid)} - vSingle: {output.VoidSingleTickets.Count()} - vParlay: {output.VoidParlayTickets.Count()}"
                        + $" - SingleResettle: {output.SingleTicketsResettle.Count()} - ParlayResettle: {output.ParlayTicketsResettle.Count()}"
                        + $" - vCricketTickets: {output.VoidCricketTickets.Count()} - CricketTicketsResettle: {output.CricketTicketsResettle.Count()}"
                        + $" - vCancelCOTickets: {output.CancelCOTickets.Count()} - vSingle14: {output.VoidSingleTickets14.Count()} "
                        + $" - vParlay14: {output.VoidParlayTickets14.Count()} - vHybridParlay: {output.VoidHybridParlayMainTickets.Count()}");

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

        var results = (await repository.FetchAsync<RequestDetailTicketModel>(criteria)).ToList();
        var recoverSingleTickets = results.Where(x => x.ValidationType is ValidationType.SingleTicketRecover or ValidationType.SingleTicketResettle);
        var recoverParlayTickets = results.Where(x => x.ValidationType is ValidationType.ParlayTicketRecover or ValidationType.ParlayTicketResettle);
        var recoverSubParlayTickets = results.Where(x => x.ValidationType is ValidationType.SubParlayTicketRecover or ValidationType.SubParlayTicketResettle);
        var recoverCricketTickets = results.Where(x => x.ValidationType is ValidationType.CricketTicketRecover or ValidationType.CricketTicketResettle);
        var recoverSingleTickets14 = results.Where(x => x.ValidationType is ValidationType.SingleTickets14Recover);
        var recoverParlayTickets14 = results.Where(x => x.ValidationType is ValidationType.ParlayTickets14Recover);
        var recoverHybridParlayMainTickets = results.Where(x => x.ValidationType is ValidationType.HybridParlayMainTicketRecover or ValidationType.HybridParlayMainTicketResettle);
        var recoverHybridParlaySubTickets = results.Where(x => x.ValidationType is ValidationType.HybridParlaySubTicketRecover or ValidationType.HybridParlaySubTicketResettle);

        var output = new CheckTicketValidationOutput
        {
            RecoverSingleTickets = recoverSingleTickets,
            RecoverParlayTickets = recoverParlayTickets,
            RecoverSubParlayTickets = recoverSubParlayTickets,
            RecoverCricketTickets = recoverCricketTickets,
            RecoverSingleTickets14 = recoverSingleTickets14,
            RecoverParlayTickets14 = recoverParlayTickets14,
            RecoverHybridParlayMainTickets = recoverHybridParlayMainTickets,
            RecoverHybridParlaySubTickets = recoverHybridParlaySubTickets
        };

        logData.Add($"{nameof(CheckTicketValidationStage)}.{nameof(ExecuteForRecover)} - rSingle: {output.RecoverSingleTickets.Count()} - rParlay: {output.RecoverParlayTickets.Count()}"
                    + $" - rSubParlay: {output.RecoverSubParlayTickets.Count()} - rCricket: {output.RecoverCricketTickets.Count()}"
                    + $" - rSingle14: {output.RecoverSingleTickets14.Count()} - rParlay14: {output.RecoverParlayTickets14.Count()}"
                    + $" - rHybridParlay: {output.RecoverHybridParlayMainTickets.Count()}");

        return output;
    }
}