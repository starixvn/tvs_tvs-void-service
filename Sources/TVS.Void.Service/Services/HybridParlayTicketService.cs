using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services;

public interface HybridParlayTicketService
{
    Task<bool> CallApi(IEnumerable<TicketDetailModel> ticketsDetail, bool isVoid);
}

public class HybridParlayTicketServiceImp(JackTimeHybridParlayService jackTimeHybridParlayService, LoggingService loggingService)
    : HybridParlayTicketService
{
    public async Task<bool> CallApi(IEnumerable<TicketDetailModel> ticketsDetail, bool isVoid)
    {
        try
        {
            var request = CreateRequest(ticketsDetail);
            return await jackTimeHybridParlayService.Execute(request, isVoid);
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync($"{nameof(HybridParlayTicketService)}.{nameof(CallApi)} Error", e);
            return false;
        }
    }

    private static HybridParlayApiModel CreateRequest(IEnumerable<TicketDetailModel> ticketsDetail)
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