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
    Task<bool> Execute(string hybridParlayTransIds, int siteDbGroupId, int voidingTypeId, int userId);
}

public class HybridParlayTicketServiceImp(
    JackTimeHybridParlayService jackTimeHybridParlayService,
    IDynamicRepository repository,
    LoggingService loggingService,
    LegOddsService legOddsService)
    : HybridParlayTicketService
{
    private readonly List<int> voidingTypeMainTicket =
    [
        VoidingTypeConstant.HybridParlay,
        VoidingTypeConstant.MainHybridParlay14,
        VoidingTypeConstant.RecoverHybridParlay,
        VoidingTypeConstant.RecoverMainHybridParlay14
    ];

    public async Task<bool> Execute(string hybridParlayTransIds, int siteDbGroupId, int voidingTypeId, int userId)
    {
        var type = GetHybridParlayType(voidingTypeId);
        try
        {
            //:: TODO: Remove this clearly Recover main ticket flow JackTime
            if (type is HybridParlayType.RecoverMain)
            {
                return true;
            }
            //:: TODO: Remove this clearly Recover main ticket flow JackTime

            if (string.IsNullOrWhiteSpace(hybridParlayTransIds))
            {
                return true;
            }

            var ticketType = voidingTypeMainTicket.Contains(voidingTypeId) ? HybridParlayTicketType.Main : HybridParlayTicketType.Sub;
            var ticketsDetail = await GetTicketsDetail(hybridParlayTransIds, ticketType);
            var request = CreateRequest(ticketsDetail, ticketType);
            var logDate = DateTimeOffset.Now.DateTime;
            var isCalled = await jackTimeHybridParlayService.Execute(request, type);
            if (IsNotUpdateLegOdds(type))
            {
                return true; // No need to call leg odds service for voiding or recovering main bets & don't care status call JackTime
            }

            return true;

            if (isCalled)
            {
                var mainTransIds = string.Join(",", ticketsDetail.Select(x => x.MainTransId));
                var result = await legOddsService.Execute(ticketsDetail.First().SubMatchId, mainTransIds, logDate, siteDbGroupId, userId);

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

    public async Task<IEnumerable<HybridParlayTicketModel>> GetTicketsDetail(string transIds, int ticketType)
    {
        var tasks = SiteDbGroupConstant.ServerList.Select(x =>
        {
            var criteria = new HybridParlayTicketCriteria
            {
                TransIds = transIds,
                TicketType = ticketType,
                SiteDbGroupId = x.Id
            };

            return repository.FetchAsync<HybridParlayTicketModel>(criteria);
        });

        var tasksResults = await Task.WhenAll(tasks);
        return tasksResults.SelectMany(data => data);
    }

    private static HybridParlayApiModel CreateRequest(IEnumerable<HybridParlayTicketModel> ticketsDetail, int ticketType)
    {
        return ticketType is HybridParlayTicketType.Main
            ? CreateRequestMainTicket(ticketsDetail)
            : CreateRequestSubTicket(ticketsDetail);
    }

    private static HybridParlayApiModel CreateRequestMainTicket(IEnumerable<HybridParlayTicketModel> ticketsDetail)
    {
        var voidBetInfos = ticketsDetail.Select(ticket => new HybridParlayVoidBetInfo
        {
            TransactionId = GetTransactionId(ticket.Transdesc),
            TransId = ticket.MainTransId,
            CustId = ticket.CustId,
        });

        return new HybridParlayApiModel
        {
            VoidBetInfos = voidBetInfos
        };
    }

    private static HybridParlayApiModel CreateRequestSubTicket(IEnumerable<HybridParlayTicketModel> ticketsDetail)
    {
        var groups = ticketsDetail.GroupBy(ticket => ticket.MainTransId);
        var voidBetInfos = groups.Select(group =>
        {
            var firstItem = group.First();
            var isVoidAllSelectInLeg = group.ToList().Count == firstItem.SelectionCount;

            return new HybridParlayVoidBetInfo
            {
                TransactionId = GetTransactionId(firstItem.Transdesc),
                TransId = firstItem.MainTransId,
                CustId = firstItem.CustId,
                MatchId = isVoidAllSelectInLeg ? firstItem.SubMatchId.ToString() : "",
                Selections = isVoidAllSelectInLeg ? [] : group.Select(CreateLegModel)
            };
        });

        return new HybridParlayApiModel
        {
            VoidBetInfos = voidBetInfos
        };
    }

    private static LegModel CreateLegModel(HybridParlayTicketModel item)
    {
        var quarter = GetQuarter(item.SubBetCheck);
        var hdp = GetHdp(item.SubHdp1, item.SubHdp2, item.SubBetTypeId);
        return new LegModel
        {
            WayId = GetWayId(item.SubBetTeam),
            MatchId = item.SubMatchId,
            MarketId = $"{item.SubMatchId}_{item.SubBetTypeId}_{quarter}_{hdp}"
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

    private static decimal GetHdp(decimal hdp1, decimal hdp2, int betType)
    {
        List<int> betTypesHandicap = [1, 7, 20, 21, 609, 612];
        return betTypesHandicap.Contains(betType) ? hdp2 - hdp1 : hdp1;
    }

    private static string GetWayId(string betTeam)
    {
        List<string> betTeamsWay1 = ["h", "a"];
        return betTeamsWay1.Contains(betTeam.ToLower()) ? "way1" : "way2";
    }

    private static string GetQuarter(string betCheck)
    {
        if (string.IsNullOrWhiteSpace(betCheck) || betCheck.Length < 2)
        {
            return "0";
        }
        return betCheck[^2..];
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