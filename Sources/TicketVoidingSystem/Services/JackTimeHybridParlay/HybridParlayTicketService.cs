using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Criteria.JackTimeHybridParlay;
using TicketVoidingSystem.Models.JackTimeHybridParlay;
using TicketVoidingSystem.Utilities;

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

    public static readonly int[] MainStatusNotCallJackTime = [TicketStatus.VoidId];
    public static readonly int[] SubStatusNotCallJackTime = [TicketStatus.DrawId];

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
            var ticketsDetail = (await GetTicketsDetail(hybridParlayTransIds, ticketType, siteDbGroupId)).ToList();
            var (requestApiModel, ticketsWaitToUpdateOdds) = CreateRequest(ticketsDetail, ticketType);
            var logDate = DateTimeOffset.Now.DateTime;

            var isCalled = ticketsWaitToUpdateOdds.Any() && await jackTimeHybridParlayService.Execute(requestApiModel, type);
            if (IsNotUpdateLegOdds(type) || !ticketsWaitToUpdateOdds.Any())
            {
                return true; // No need to call leg odds service for voiding or recovering main bets & don't care status call JackTime
            }

            if (isCalled)
            {
                var result = await legOddsService.Execute(ticketsDetail[0].SubMatchId, logDate, siteDbGroupId, userId, ticketsWaitToUpdateOdds);

                return result;
            }

            return false;
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync($"{nameof(HybridParlayTicketService)}.{nameof(Execute)} Error", e);
            return IsNotUpdateLegOdds(type);
        }
    }

    public async Task<IEnumerable<HybridParlayTicketModel>> GetTicketsDetail(string transIds, int ticketType, int siteDbGroupId)
    {
        var criteria = new HybridParlayTicketCriteria
        {
            TransIds = transIds,
            TicketType = ticketType,
            SiteDbGroupId = siteDbGroupId
        };
        var result = await repository.FetchAsync<HybridParlayTicketModel>(criteria);
        await loggingService.InfoAsync($"Hybrid Parlay - GetTicketsDetail: {transIds} \n" +
                                       $"ticketType: {ticketType} \n" +
                                       $"siteDbGroupId: {siteDbGroupId} \n" +
                                       $"response: {Helper.JsonSerializerUnsafeRelaxed(result)}");

        return result;
    }

    private static (HybridParlayApiModel ApiModel, List<LegOddsModel> TicketsWaitToUpdateOdds) CreateRequest(
        IEnumerable<HybridParlayTicketModel> ticketsDetail, int ticketType)
    {
        return ticketType is HybridParlayTicketType.Main
            ? CreateRequestMainTicket(ticketsDetail)
            : CreateRequestSubTicket(ticketsDetail);
    }

    private static (HybridParlayApiModel ApiModel, List<LegOddsModel> TicketsWaitToUpdateOdds) CreateRequestMainTicket(
        IEnumerable<HybridParlayTicketModel> ticketsDetail)
    {
        var voidBetInfos = ticketsDetail.Select(ticket => new HybridParlayVoidBetInfo
        {
            TransactionId = GetTransactionId(ticket.Transdesc),
            TransId = ticket.MainTransId.ToString(),
            CustId = ticket.CustId,
        });

        return (new HybridParlayApiModel { VoidBetInfos = voidBetInfos }, []);
    }

    private static (HybridParlayApiModel ApiModel, List<LegOddsModel> TicketsWaitToUpdateOdds) CreateRequestSubTicket(
        IEnumerable<HybridParlayTicketModel> ticketsDetail)
    {
        var ticketWillBeCallJackTime = ticketsDetail.Where(x
            => !MainStatusNotCallJackTime.Contains(x.StatusId) &&
               !SubStatusNotCallJackTime.Contains(x.SubStatusId)
        );
        var voidBetInfosList = new List<HybridParlayVoidBetInfo>();
        var ticketsWaitToUpdateOdds = new List<LegOddsModel>();
        var groups = ticketWillBeCallJackTime.GroupBy(ticket => ticket.MainTransId);
        foreach (var group in groups)
        {
            var firstItem = group.First();
            var type = GetType(group.ToList());
            if (type is HybridParlayActionType.VoidSelections)
            {
                ticketsWaitToUpdateOdds.Add(new LegOddsModel { MatchId = firstItem.SubMatchId, TransId = firstItem.MainTransId });
            }
            var model = new HybridParlayVoidBetInfo
            {
                TransactionId = GetTransactionId(firstItem.Transdesc),
                TransId = firstItem.MainTransId.ToString(),
                CustId = firstItem.CustId,
                MatchId = "",
                Type = GetType(group.ToList()),
                Selections = group.Select(CreateLegModel)
            };

            voidBetInfosList.Add(model);
        }

        return (new HybridParlayApiModel { VoidBetInfos = voidBetInfosList }, ticketsWaitToUpdateOdds);
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

    private static byte GetType(IEnumerable<HybridParlayTicketModel> legs)
    {
        var countTickets = legs.FirstOrDefault()?.SelectionCount ?? 0;
        return countTickets == 1
            ? HybridParlayActionType.VoidLegs
            : HybridParlayActionType.VoidSelections;
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

    private static string GetHdp(decimal hdp1, decimal hdp2, int betType)
    {
        List<int> betTypesHandicap = [1, 7, 20, 21, 609, 612];
        var value = betTypesHandicap.Contains(betType) ? hdp2 - hdp1 : hdp1;

        return value.ToString("G29", CultureInfo.InvariantCulture);
    }

    private static string GetWayId(string betTeam)
    {
        List<string> betTeamsWay1 = ["h", "o"];
        return betTeamsWay1.Contains(betTeam.ToLowerInvariant()) ? "way1" : "way2";
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