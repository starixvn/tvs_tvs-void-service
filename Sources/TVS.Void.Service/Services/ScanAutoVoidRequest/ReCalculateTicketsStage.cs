using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Models;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest
{
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
            // TODO: Check if this step fail -> re execute, write log
            if (singleTicketsResettle.Any() || parlayTicketsResettle.Any())
            {
                await UnMarkTickets(siteDbGroupId, singleTicketsResettle, parlayTicketsResettle, logData);
                await ReCalculateTickets(siteDbGroupId, singleTicketsResettle, parlayTicketsResettle, logData);
                await UpdateIsProcessed(singleTicketsResettle, parlayTicketsResettle, logData);
            }
        }

        private async Task UnMarkTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle,
            IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, ICollection<string> logData)
        {
            var ticketList = singleTicketsResettle.Select(x => x.TransId).Union(parlayTicketsResettle.Select(x => x.TransId));
            var transIdText = string.Join(",", ticketList);
            logData.Add($"UnMarkTickets: {transIdText}");
            var criteria = new UnMarkTicketsCriteria
            {
                LogActionId = 90510,
                SiteDbGroupId = siteDbGroupId,
                TransIds = transIdText,
                UserId = AutoVoidConstant.ActorId,
                Ip = AutoVoidConstant.IPAddress
            };

            // TODO: Handle when error
            var result = await repository.FetchAsync<Result>(criteria);
            logData.Add($"UnMarkTickets-Result: {JsonSerializer.Serialize(result)}");
        }

        private async Task ReCalculateTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle,
            IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, ICollection<string> logData)
        {
            await ReCalculateSingleTicketsByMatch(siteDbGroupId, singleTicketsResettle, logData);
            await ReCalculateParlayTickets(siteDbGroupId, parlayTicketsResettle, logData);
        }

        private async Task ReCalculateSingleTicketsByMatch(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, ICollection<string> logData)
        {
            if (singleTicketsResettle.Any())
            {
                var matches = singleTicketsResettle.Select(x => x.MatchId).Distinct();
                foreach (var matchId in matches)
                {
                    var singleTransIdList = singleTicketsResettle.Where(x => x.MatchId == matchId)
                        .Select(x => x.TransId);
                    var singleTransIdText = string.Join(",", singleTransIdList);
                    logData.Add($"ReCalcSingle - MatchId:{matchId} - SingleTransId:{singleTransIdText}");
                    await ExecuteReCalculateTickets(siteDbGroupId, matchId, singleTransIdText);
                }
            }
        }

        private async Task ReCalculateParlayTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, ICollection<string> logData)
        {
            if (parlayTicketsResettle.Any())
            {
                const int parlayMatchId = 29;
                var parlayTransIdList = parlayTicketsResettle.Where(x => x.MatchId == parlayMatchId).Select(x => x.TransId);
                var parlayTransIdText = string.Join(",", parlayTransIdList);
                logData.Add($"ReCalcParlay - MatchId:{parlayMatchId} - ParlayTransId:{parlayTransIdText}");
                await ExecuteReCalculateTickets(siteDbGroupId, parlayMatchId, parlayTransIdText);
            }
        }

        private async Task ExecuteReCalculateTickets(int siteDbGroupId, int matchId, string transIds)
        {
            var criteria = new ReCalculateTicketsCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                MatchId = matchId,
                TransIds = transIds,
                UserId = AutoVoidConstant.ActorId,
                Ip = AutoVoidConstant.IPAddress
            };

            // TODO: Handle when error
            await repository.ExecuteAsync(criteria);
        }

        private async Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, ICollection<string> logData)
        {
            await requestTicketService.UpdateIsProcessed(singleTicketsResettle, parlayTicketsResettle);
            logData.Add($"{nameof(RequestTicketService.UpdateIsProcessed)}-AfterReCalc-OK");
        }
    }
}