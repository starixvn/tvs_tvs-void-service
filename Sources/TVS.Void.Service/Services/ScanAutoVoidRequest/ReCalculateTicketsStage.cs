using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface ReCalculateTicketsStage
{
    Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle,
        IEnumerable<RequestDetailTicketModel> parlayTicketsResettle,
        IEnumerable<RequestDetailTicketModel> cricketTicketsResettle, ICollection<string> logData);
}

public class ReCalculateTicketsStageImp : ReCalculateTicketsStage
{
    private readonly RequestTicketService requestTicketService;
    private readonly IDynamicRepository repository;

    public ReCalculateTicketsStageImp(IDynamicRepository repository, RequestTicketService requestTicketService)
    {
        this.repository = repository;
        this.requestTicketService = requestTicketService;
    }

    public async Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle,
        IEnumerable<RequestDetailTicketModel> parlayTicketsResettle,
        IEnumerable<RequestDetailTicketModel> cricketTicketsResettle, ICollection<string> logData)
    {
        await UnMarkAndReCalculateSingleTicketsByMatch(siteDbGroupId, singleTicketsResettle, cricketTicketsResettle, logData);
        await UnMarkAndReCalculateMainParlayTickets(siteDbGroupId, parlayTicketsResettle, logData);
    }

    private async Task UnMarkAndReCalculateSingleTicketsByMatch(int siteDbGroupId,
        IEnumerable<RequestDetailTicketModel> singleTicketsResettle,
        IEnumerable<RequestDetailTicketModel> cricketTicketsResettle, ICollection<string> logData)
    {
        if (singleTicketsResettle.Any())
        {
            var ticketCanBeResettle = singleTicketsResettle.Union(cricketTicketsResettle);
            var matches = ticketCanBeResettle.Select(x => x.MatchId).Distinct();
            foreach (var matchId in matches)
            {
                var ticketCanBeResettleInCurrentMatch = ticketCanBeResettle.Where(x => x.MatchId == matchId);
                var transIdText = string.Join(",", ticketCanBeResettleInCurrentMatch.Select(x => x.TransId));

                var result = await ExecuteReCalculateTickets(siteDbGroupId, matchId, transIdText);
                logData.Add($"UnMarkAndReCalcSingle - MatchId:{matchId} - SingleTransId:{transIdText} - Result:{JsonSerializer.Serialize(result)}");
                await requestTicketService.UpdateIsProcessed(singleTicketsResettle, new List<RequestDetailTicketModel>(), cricketTicketsResettle);
                logData.Add($"{nameof(RequestTicketService.UpdateIsProcessed)}-AfterUnmarkAndReCalcSingle-OK");
            }
        }
    }

    private async Task UnMarkAndReCalculateMainParlayTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, ICollection<string> logData)
    {
        if (parlayTicketsResettle.Any())
        {
            const int mainParlayMatchId = 29;
            var mainParlayTickets = parlayTicketsResettle.Where(x => x.MatchId == mainParlayMatchId);
            var parlayTransIdText = string.Join(",", mainParlayTickets.Select(x => x.TransId));

            var result = await ExecuteReCalculateTickets(siteDbGroupId, mainParlayMatchId, parlayTransIdText);
            logData.Add($"UnMarkAndReCalcMainParlay - MatchId:{mainParlayMatchId} - MainParlayTransId:{parlayTransIdText} - Result:{JsonSerializer.Serialize(result)}");
            await requestTicketService.UpdateIsProcessed(new List<RequestDetailTicketModel>(), mainParlayTickets, new List<RequestDetailTicketModel>());
            logData.Add($"{nameof(RequestTicketService.UpdateIsProcessed)}-AfterUnmarkAndReCalcMainParlay-OK");
        }
    }

    private async Task<IEnumerable<UnMarkReCalculateResult>> ExecuteReCalculateTickets(int siteDbGroupId, int matchId, string transIds)
    {
        var criteria = new UnMarkAndReCalculateTicketsCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            MatchId = matchId,
            TransIds = transIds,
            UserId = AutoVoidConstant.ActorId,
            Ip = AutoVoidConstant.IPAddress
        };

        return await repository.FetchAsync<UnMarkReCalculateResult>(criteria);
    }
}