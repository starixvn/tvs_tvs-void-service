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
    Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, ICollection<string> logData);
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

    public async Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle,
        ICollection<string> logData)
    {
        await UnMarkAndReCalculateSingleTicketsByMatch(siteDbGroupId, singleTicketsResettle, logData);
        await UnMarkAndReCalculateMainParlayTickets(siteDbGroupId, parlayTicketsResettle, logData);
    }

    private async Task UnMarkAndReCalculateSingleTicketsByMatch(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, ICollection<string> logData)
    {
        if (singleTicketsResettle.Any())
        {
            var matches = singleTicketsResettle.Select(x => x.MatchId).Distinct();
            foreach (var matchId in matches)
            {
                var singleTicketsInCurrentMatch = singleTicketsResettle.Where(x => x.MatchId == matchId);
                var singleTransIdText = string.Join(",", singleTicketsInCurrentMatch.Select(x => x.TransId));

                var result = await ExecuteReCalculateTickets(siteDbGroupId, matchId, singleTransIdText);
                logData.Add($"UnMarkAndReCalcSingle - MatchId:{matchId} - SingleTransId:{singleTransIdText} - Result:{JsonSerializer.Serialize(result)}");
                await requestTicketService.UpdateIsProcessed(singleTicketsResettle, null);
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
            logData.Add($"UnMarkAndReCalcMainParlay - MatchId:{mainParlayTickets} - MainParlayTransId:{parlayTransIdText} - Result:{JsonSerializer.Serialize(result)}");
            await requestTicketService.UpdateIsProcessed(null, mainParlayTickets);
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