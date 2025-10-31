using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Criteria.JackTimeHybridParlay;
using TicketVoidingSystem.Models.JackTimeHybridParlay;

namespace TicketVoidingSystem.Services.JackTimeHybridParlay;

public interface HybridParlayTicketService
{
    Task<bool> Execute(string hybridParlayTransIds, int siteDbGroupId, int voidingTypeId);
}

public class HybridParlayTicketServiceImp(
    JackTimeHybridParlayService jackTimeHybridParlayService,
    IDynamicRepository repository,
    LoggingService loggingService,
    LegOddsService legOddsService)
    : HybridParlayTicketService
{
    public async Task<bool> Execute(string hybridParlayTransIds, int siteDbGroupId, int voidingTypeId)
    {
        var type = GetHybridParlayType(voidingTypeId);
        try
        {
            if (string.IsNullOrWhiteSpace(hybridParlayTransIds))
            {
                return false;
            }

            var ticketsDetail = await GetTicketsDetail(hybridParlayTransIds);
            var request = CreateRequest(ticketsDetail);
            var logDate = DateTimeOffset.Now.DateTime;
            var isCalled = await jackTimeHybridParlayService.Execute(request, type);
            if (IsNotUpdateLegOdds(type))
            {
                return true; // No need to call leg odds service for voiding or recovering main bets & don't care status call JackTime
            }

            if (isCalled)
            {
                var mainTransIds = string.Join(",", ticketsDetail.Select(x => x.MainTransId));
                var result = await legOddsService.Execute(ticketsDetail.First().SubMatchId, mainTransIds, logDate);

                return result.IsSuccessful;
            }

            return false;
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync($"{nameof(HybridParlayTicketService)}.{nameof(Execute)} Error", e);
            return IsNotUpdateLegOdds(type);
        }
    }

    public async Task<IEnumerable<HybridParlayTicketModel>> GetTicketsDetail(string transIds)
    {
        var criteria = new HybridParlayTicketCriteria
        {
            TransIds = transIds
        };

        return await repository.FetchAsync<HybridParlayTicketModel>(criteria);
    }

    private static HybridParlayApiModel CreateRequest(IEnumerable<HybridParlayTicketModel> ticketsDetail)
    {
        var groups = ticketsDetail.GroupBy(ticket => ticket.MainTransId);
        var voidBetInfos = groups.Select(group =>
        {
            var mainTransId = group.Key;
            var firstItem = group.First();
            return new HybridParlayVoidBetInfo
            {
                TransactionId = GetTransactionId(firstItem.Transdesc),
                TransId = firstItem.MainTransId,
                CustId = firstItem.CustId,
                Legs = group.Select(item => new LegModel
                {
                    WayId = item.SubBetTeam,
                    MatchId = item.SubMatchId,
                    MarketId = $"{item.SubMatchId}_{item.SubBetTypeId}_{item.SubHdp1}"
                })
            };
        });

        return new HybridParlayApiModel
        {
            VoidBetInfos = voidBetInfos
        };
    }

    private static bool IsNotUpdateLegOdds(HybridParlayType type)
    {
        return type is HybridParlayType.VoidMain or HybridParlayType.RecoverMain;
    }

    private static string GetTransactionId(string transdesc)
    {
        using var doc = JsonDocument.Parse(transdesc);
        return doc.RootElement.GetProperty("id").GetString();
    }

    private static HybridParlayType GetHybridParlayType(int voidingTypeId)
    {
        return voidingTypeId switch
        {
            VoidingTypeConstant.HybridParlay or VoidingTypeConstant.MainHybridParlay14 => HybridParlayType.VoidMain,
            VoidingTypeConstant.RecoverHybridParlay or VoidingTypeConstant.RecoverMainHybridParlay14 => HybridParlayType.RecoverMain,
            VoidingTypeConstant.SubHybridParlay => HybridParlayType.VoidSub,
            VoidingTypeConstant.RecoverSubHybridParlay => HybridParlayType.RecoverSub,
            _ => HybridParlayType.VoidMain
        };
    }
}