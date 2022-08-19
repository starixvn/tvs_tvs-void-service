using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface ReCalculateTicketsStage
    {
        Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle);
    }

    public class ReCalculateTicketsStageImp : ReCalculateTicketsStage
    {
        private readonly IDynamicRepository repository;

        public ReCalculateTicketsStageImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public async Task Execute(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle)
        {
            // TODO: Check if this step fail -> re execute, write log
            if (singleTicketsResettle.Any() || parlayTicketsResettle.Any())
            {
                await UnMarkTickets(siteDbGroupId, singleTicketsResettle, parlayTicketsResettle);
                await ReCalculateTickets(siteDbGroupId, singleTicketsResettle, parlayTicketsResettle);
            }
        }

        private async Task UnMarkTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle)
        {
            var ticketList = singleTicketsResettle.Select(x => x.TransId).Union(parlayTicketsResettle.Select(x => x.TransId));
            var criteria = new UnMarkTicketsCriteria
            {
                SiteDbGroupId = siteDbGroupId,
                TransIds = string.Join(",", ticketList),
                UserId = AutoVoidConstant.ActorId,
                Ip = AutoVoidConstant.IPAddress
            };

            // TODO: Handle when error
            await repository.ExecuteAsync(criteria);
        }

        private async Task ReCalculateTickets(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> singleTicketsResettle, IEnumerable<RequestDetailTicketModel> parlayTicketsResettle)
        {
            if (singleTicketsResettle.Any())
            {
                var firstSingleTicket = singleTicketsResettle.First();
                var singleTransIdList = singleTicketsResettle.Where(x => x.MatchId == firstSingleTicket.MatchId)
                    .Select(x => x.TransId);
                await ExecuteReCalculateTickets(siteDbGroupId, firstSingleTicket.MatchId, string.Join(",", singleTransIdList));
            }

            if (parlayTicketsResettle.Any())
            {
                const int parlayMatchId = 29;
                var parlayTransIdList = parlayTicketsResettle.Where(x => x.MatchId == parlayMatchId).Select(x => x.TransId);
                await ExecuteReCalculateTickets(siteDbGroupId, parlayMatchId, string.Join(",", parlayTransIdList));
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