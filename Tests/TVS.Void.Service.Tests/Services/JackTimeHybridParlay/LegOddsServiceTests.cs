using Fanex.Data.Repository;
using NSubstitute;
using TicketVoidingSystem.Criteria.JackTimeHybridParlay;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Models.JackTimeHybridParlay;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.Services.JackTimeHybridParlay;

namespace TVS.Void.Service.Tests.Services.JackTimeHybridParlay;

public class LegOddsServiceTests
{
    private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();
    private readonly LoggingService loggingService = Substitute.For<LoggingService>();

    [Fact]
    public async Task Execute_Success_WhenLegOddsExistsAndUpdateSuccess()
    {
        // Arrange
        const int fakeMatchId = 1;
        const string fakeTransIds = "123,456";
        var fakeLogDate = DateTime.Now;
        var fakeLegOdds = new List<LegOddsModel>
        {
            new()
            {
                MatchId = fakeMatchId,
                BetId = 100,
                Odds = 1.5m
            }
        };
        var successResult = new Result { ErrorCode = 0, ErrorMsg = string.Empty };

        repository.FetchAsync<LegOddsModel>(Arg.Any<HybridParlayGetLegOddsCriteria>())
            .ReturnsForAnyArgs(fakeLegOdds);
        repository.GetAsync<Result>(Arg.Any<HybridParlayUpdateLegOddsCriteria>())
            .ReturnsForAnyArgs(successResult);
        var service = new LegOddsServiceImp(repository, loggingService);

        // Act
        var actual = await service.Execute(fakeMatchId, fakeTransIds, fakeLogDate);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(0, actual.ErrorCode);
        Assert.True(string.IsNullOrEmpty(actual.ErrorMsg));
        await repository.Received(1).FetchAsync<LegOddsModel>(
            Arg.Is<HybridParlayGetLegOddsCriteria>(x =>
                x.MatchId == fakeMatchId && x.TransIds == fakeTransIds && x.LogDate == fakeLogDate));
        await repository.Received(1).GetAsync<Result>(Arg.Any<HybridParlayUpdateLegOddsCriteria>());
    }

    [Fact]
    public async Task Execute_ReturnsError_WhenNoLegOdds()
    {
        // Arrange
        const int fakeMatchId = 1;
        const string fakeTransIds = "123,456";
        var fakeLogDate = DateTime.Now;
        var emptyLegOdds = Enumerable.Empty<LegOddsModel>();

        repository.FetchAsync<LegOddsModel>(Arg.Any<HybridParlayGetLegOddsCriteria>())
            .ReturnsForAnyArgs(emptyLegOdds);
        var service = new LegOddsServiceImp(repository, loggingService);

        // Act
        var actual = await service.Execute(fakeMatchId, fakeTransIds, fakeLogDate);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(1, actual.ErrorCode);
        Assert.Equal("Cannot get legOdds. Please try again!", actual.ErrorMsg);
        await repository.DidNotReceive().GetAsync<Result>(Arg.Any<HybridParlayUpdateLegOddsCriteria>());
    }

    [Fact]
    public async Task Execute_ReturnsError_WhenExceptionOccurs()
    {
        // Arrange
        const int fakeMatchId = 1;
        const string fakeTransIds = "123,456";
        var fakeLogDate = DateTime.Now;
        var exception = new Exception("Test exception");

        repository.FetchAsync<LegOddsModel>(Arg.Any<HybridParlayGetLegOddsCriteria>())
            .Returns(Task.FromException<IEnumerable<LegOddsModel>>(exception));
        var service = new LegOddsServiceImp(repository, loggingService);

        // Act
        var actual = await service.Execute(fakeMatchId, fakeTransIds, fakeLogDate);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(1, actual.ErrorCode);
        Assert.Equal("Test exception", actual.ErrorMsg);
        await loggingService.Received(1).ErrorAsync("Test exception", exception);
    }

    [Fact]
    public async Task GetLegOdds_ShouldCallRepositoryWithCorrectCriteria()
    {
        // Arrange
        const int fakeMatchId = 1;
        const string fakeTransIds = "123,456";
        var fakeLogDate = DateTime.Now;
        var fakeLegOdds = new List<LegOddsModel>
        {
            new()
            {
                MatchId = fakeMatchId,
                BetId = 100,
                Odds = 1.5m
            }
        };

        repository.FetchAsync<LegOddsModel>(Arg.Any<HybridParlayGetLegOddsCriteria>())
            .ReturnsForAnyArgs(fakeLegOdds);
        var service = new LegOddsServiceImp(repository, loggingService);

        // Act
        var actual = await service.GetLegOdds(fakeMatchId, fakeTransIds, fakeLogDate);

        // Assert
        await repository.Received(1).FetchAsync<LegOddsModel>(
            Arg.Is<HybridParlayGetLegOddsCriteria>(x =>
                x.MatchId == fakeMatchId && x.TransIds == fakeTransIds && x.LogDate == fakeLogDate));
        Assert.NotNull(actual);
        Assert.Single(actual);
    }

    [Fact]
    public async Task UpdateLegOdds_ShouldCallRepositoryWithCorrectCriteria()
    {
        // Arrange
        var fakeLegOdds = new List<LegOddsModel>
        {
            new()
            {
                MatchId = 1,
                BetId = 100,
                Odds = 1.5m
            },
            new()
            {
                MatchId = 2,
                BetId = 200,
                Odds = 2.0m
            }
        };
        var successResult = new Result { ErrorCode = 0, ErrorMsg = string.Empty };

        repository.GetAsync<Result>(Arg.Any<HybridParlayUpdateLegOddsCriteria>())
            .ReturnsForAnyArgs(successResult);
        var service = new LegOddsServiceImp(repository, loggingService);

        // Act
        var actual = await service.UpdateLegOdds(fakeLegOdds);

        // Assert
        await repository.Received(1).GetAsync<Result>(
            Arg.Is<HybridParlayUpdateLegOddsCriteria>(x =>
                !string.IsNullOrWhiteSpace(x.LegOddsJson)));
        Assert.NotNull(actual);
        Assert.Equal(0, actual.ErrorCode);
    }

    [Fact]
    public void BuildErrorResult_ShouldReturnErrorResult()
    {
        // Arrange
        const string errorMsg = "Test error message";

        // Act
        var actual = LegOddsServiceImp.BuildErrorResult(errorMsg);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(1, actual.ErrorCode);
        Assert.Equal(errorMsg, actual.ErrorMsg);
    }
}