using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Fanex.Logging;
using Polly;
using TicketVoidingSystem.Criteria.JackTimeHybridParlay;
using TicketVoidingSystem.Models.JackTimeHybridParlay;
using TicketVoidingSystem.Utilities;

namespace TicketVoidingSystem.Services.JackTimeHybridParlay;

public interface LegOddsService
{
    Task<bool> Execute(int matchId, DateTime logDate, int siteDbGroupId, int userId, IEnumerable<LegOddsModel> ticketsWaitToUpdateOdds);
}

public class LegOddsServiceImp(IDynamicRepository repository, LoggingService loggingService)
    : LegOddsService
{
    public async Task<bool> Execute(int matchId, DateTime logDate, int siteDbGroupId, int userId, IEnumerable<LegOddsModel> ticketsWaitToUpdateOdds)
    {
        try
        {
            var legOdds = await RetryForGetLegOdds(matchId, logDate, ticketsWaitToUpdateOdds);
            if (legOdds.Any())
            {
                return await UpdateLegOdds(legOdds, matchId, siteDbGroupId, userId);
            }

            return false;
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync(e.Message, e);

            return false;
        }
    }

    public async Task<IEnumerable<LegOddsModel>> RetryForGetLegOdds(int matchId, DateTime logDate,
        IEnumerable<LegOddsModel> ticketsWaitToUpdateOdds)
    {
        const byte retryCount = 10;
        const byte retryTimes = 3;
        var legOdds = Enumerable.Empty<LegOddsModel>();
        var expectedTransIds = ticketsWaitToUpdateOdds.Select(t => t.TransId).Distinct().ToHashSet();
        await Policy
            .HandleResult<bool>(success => !success)
            .WaitAndRetryAsync(retryCount,
                _ => TimeSpan.FromSeconds(retryTimes))
            .ExecuteAsync(async () =>
            {
                legOdds = await GetLegOdds(matchId, logDate, ticketsWaitToUpdateOdds);

                var receivedTransIds = legOdds
                    .Select(o => o.TransId)
                    .Distinct()
                    .ToHashSet();

                var allMatched = expectedTransIds.SetEquals(receivedTransIds);
                return allMatched;
            });

        var receivedFinalTransIds = legOdds.Select(x => x.TransId).Distinct().ToHashSet();
        var isReceivedAllOdds = expectedTransIds.SetEquals(receivedFinalTransIds);

        if (isReceivedAllOdds)
        {
            return legOdds;
        }

        await loggingService.InfoAsync($"Timeout or don't received enough number of ODDs \n" +
                                       $"Requested: {Helper.JsonSerializerUnsafeRelaxed(ticketsWaitToUpdateOdds)} \n" +
                                       $"Received: {Helper.JsonSerializerUnsafeRelaxed(legOdds)} \n");
        return [];
    }

    public async Task<IEnumerable<LegOddsModel>> GetLegOdds(int matchId, DateTime logDate, IEnumerable<LegOddsModel> ticketsWaitToUpdateOdds)
    {
        var criteria = new HybridParlayGetLegOddsCriteria
        {
            MatchId = matchId,
            TransIds = string.Join(",", ticketsWaitToUpdateOdds.Select(x => x.TransId)),
            LogDate = logDate
        };
        await LogService.InfoAsync("Retry for get odds:" + JsonSerializer.Serialize(criteria));

        return await repository.FetchAsync<LegOddsModel>(criteria);
    }

    public async Task<bool> UpdateLegOdds(IEnumerable<LegOddsModel> legOdds, int matchId, int siteDbGroupId, int userId)
    {
        var jsonLegOdds = JsonSerializer.Serialize(legOdds);
        var criteria = new HybridParlayUpdateLegOddsCriteria
        {
            LegOddsJson = jsonLegOdds,
            MatchId = matchId,
            Creator = userId,
            SiteDbGroupId = siteDbGroupId
        };

        await repository.ExecuteAsync(criteria);
        await loggingService.InfoAsync($"Update leg odds: {jsonLegOdds} \nErrorCode: {criteria.ErrCode} \nErrMsg: {criteria.ErrMsg}");

        return criteria.ErrCode == 0 || string.IsNullOrWhiteSpace(criteria.ErrMsg);
    }
}