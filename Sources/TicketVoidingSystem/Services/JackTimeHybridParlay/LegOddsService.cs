using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Polly;
using TicketVoidingSystem.Criteria.JackTimeHybridParlay;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Models.JackTimeHybridParlay;

namespace TicketVoidingSystem.Services.JackTimeHybridParlay;

public interface LegOddsService
{
    Task<Result> Execute(int matchId, string transIds, DateTime logDate, int siteDbGroupId, int userId);
}

public class LegOddsServiceImp(IDynamicRepository repository, LoggingService loggingService)
    : LegOddsService
{
    public async Task<Result> Execute(int matchId, string transIds, DateTime logDate, int siteDbGroupId, int userId)
    {
        try
        {
            var legOdds = await RetryForGetLegOdds(matchId, transIds, logDate);
            if (legOdds.Any())
            {
                return await UpdateLegOdds(legOdds, matchId, siteDbGroupId, userId);
            }

            return BuildErrorResult("Cannot get legOdds. Please try again!");
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync(e.Message, e);

            return BuildErrorResult(e.Message);
        }
    }

    public async Task<IEnumerable<LegOddsModel>> RetryForGetLegOdds(int matchId, string transIds, DateTime logDate)
    {
        const byte retryCount = 10;
        const byte retryTimes = 5;
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

        return await repository.FetchAsync<LegOddsModel>(criteria);
    }

    public async Task<Result> UpdateLegOdds(IEnumerable<LegOddsModel> legOdds, int matchId, int siteDbGroupId, int userId)
    {
        var criteria = new HybridParlayUpdateLegOddsCriteria
        {
            LegOddsJson = JsonSerializer.Serialize(legOdds),
            MatchId = matchId,
            Creator = userId,
            SiteDbGroupId = siteDbGroupId
        };

        return await repository.GetAsync<Result>(criteria);
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