using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Fanex.Logging;
using Polly;
using TicketVoidingSystem.Criteria.JackTimeHybridParlay;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Models.JackTimeHybridParlay;

namespace TicketVoidingSystem.Services.JackTimeHybridParlay;

public interface LegOddsService
{
    Task<bool> Execute(int matchId, string transIds, DateTime logDate, int siteDbGroupId, int userId);
}

public class LegOddsServiceImp(IDynamicRepository repository, LoggingService loggingService)
    : LegOddsService
{
    public async Task<bool> Execute(int matchId, string transIds, DateTime logDate, int siteDbGroupId, int userId)
    {
        try
        {
            var legOdds = await RetryForGetLegOdds(matchId, transIds, logDate);
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

    public async Task<IEnumerable<LegOddsModel>> RetryForGetLegOdds(int matchId, string transIds, DateTime logDate)
    {
        const byte retryCount = 5;
        const byte retryTimes = 3;
        var legOdds = Enumerable.Empty<LegOddsModel>();
        await Policy
            .HandleResult<bool>(c => !c)
            .WaitAndRetryAsync(retryCount,
                _ => TimeSpan.FromSeconds(retryTimes))
            .ExecuteAsync(async () =>
            {
                legOdds = await GetLegOdds(matchId, transIds, logDate);
                return legOdds.Any();
            });

        return legOdds;
    }

    public async Task<IEnumerable<LegOddsModel>> GetLegOdds(int matchId, string transIds, DateTime logDate)
    {
        var criteria = new HybridParlayGetLegOddsCriteria
        {
            MatchId = matchId,
            TransIds = transIds,
            LogDate = logDate
        };
        LogService.InfoAsync("Retry for get odds:" + JsonSerializer.Serialize(criteria));

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

    public static Result BuildErrorResult(string errorMsg)
    {
        return new Result
        {
            ErrorMsg = errorMsg,
            ErrorCode = 1
        };
    }
}