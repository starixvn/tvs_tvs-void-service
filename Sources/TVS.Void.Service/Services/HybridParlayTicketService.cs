using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services;

public interface HybridParlayTicketService
{
    Task<bool> Execute(string hybridParlayTransIds, int siteDbGroupId, bool isVoid);
}

public class HybridParlayTicketServiceImp(JackTimeHybridParlayService jackTimeHybridParlayService, DynamicRepository repository, LoggingService loggingService)
    : HybridParlayTicketService
{
    public async Task<bool> Execute(string hybridParlayTransIds, int siteDbGroupId, bool isVoid)
    {
        try
        {
            var ticketsDetail = await GetTicketsDetail(siteDbGroupId, hybridParlayTransIds);
            var request = CreateRequest(ticketsDetail);
            return await jackTimeHybridParlayService.Execute(request, isVoid);
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync($"{nameof(HybridParlayTicketService)}.{nameof(Execute)} Error", e);
            return false;
        }
    }

    private async Task<IEnumerable<HybridParlayTicketModel>> GetTicketsDetail(int siteDbGroupId, string transIds)
    {
        var criteria = new GetTicketsDetailCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            TransIds = transIds
        };

        return await repository.FetchAsync<HybridParlayTicketModel>(criteria);
    }

    private static HybridParlayApiModel CreateRequest(IEnumerable<HybridParlayTicketModel> ticketsDetail)
    {
        var voidBetInfos = ticketsDetail.GroupBy(ticket => ticket.MainTransId).Select(group => new VoidBetInfo
        {
            Bet = { TransId = group.Key },
            Legs = group.Select(item => new LegModel
            {
                Way = { Id = item.SubBetTeam },
                Market = { MatchId = item.SubMatchId, MarketId = $"{item.SubMatchId}_{item.SubBetTypeId}_{item.SubHdp1}" }
            })
        });

        return new HybridParlayApiModel
        {
            VoidBetInfos = voidBetInfos
        };
    }
}