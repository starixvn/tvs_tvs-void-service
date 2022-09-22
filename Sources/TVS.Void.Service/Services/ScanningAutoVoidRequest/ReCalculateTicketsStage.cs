using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Models;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface ReCalculateTicketsStage
    {
        Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, StringBuilder logData);
    }

    public class ReCalculateTicketsStageImp : ReCalculateTicketsStage
    {
        private readonly IDynamicRepository repository;

        public ReCalculateTicketsStageImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, StringBuilder logData)
        {
            // TODO: Check if this step fail -> re execute, write log
            if (singleTicketsResettle.Any() || parlayTicketsResettle.Any())
            {
                await UnMarkTickets(siteDbGroupId, singleTicketsResettle, parlayTicketsResettle, logData);
                await ReCalculateTickets(siteDbGroupId, singleTicketsResettle, parlayTicketsResettle, logData);
            }
        }

        private async Task UnMarkTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle,
            IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, StringBuilder logData)
        {
            var ticketList = singleTicketsResettle.Select(x => x.TransId).Union(parlayTicketsResettle.Select(x => x.TransId));
            var transIdText = string.Join(",", ticketList);
            logData.AppendLine($"UnMarkTickets: {transIdText}");
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
            logData.AppendLine($"UnMarkTickets-Result: {JsonSerializer.Serialize(result)}");
        }

        private async Task ReCalculateTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle,
            IEnumerable<RequestDetailTicketModel> parlayTicketsResettle, StringBuilder logData)
        {
            if (singleTicketsResettle.Any())
            {
                var firstSingleTicket = singleTicketsResettle.First();
                var singleTransIdList = singleTicketsResettle.Where(x => x.MatchId == firstSingleTicket.MatchId)
                    .Select(x => x.TransId);
                var singleTransIdText = string.Join(",", singleTransIdList);
                logData.AppendLine($"ReCalcSingle - MatchId:{firstSingleTicket.MatchId} - SingleTransId:{singleTransIdText}");
                await ExecuteReCalculateTickets(siteDbGroupId, firstSingleTicket.MatchId, singleTransIdText);
            }

            if (parlayTicketsResettle.Any())
            {
                const int parlayMatchId = 29;
                var parlayTransIdList = parlayTicketsResettle.Where(x => x.MatchId == parlayMatchId).Select(x => x.TransId);
                var parlayTransIdText = string.Join(",", parlayTransIdList);
                logData.AppendLine($"ReCalcParlay - MatchId:{parlayMatchId} - ParlayTransId:{parlayTransIdText}");
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
    }
}