using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.CheckTicketStatus;

namespace TVS.Void.Service.Tests.Services.CheckTicketStatus;

public class InsertAutoVoidQueueStageImpTests
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly InsertAutoVoidQueueStageImp service;

    public InsertAutoVoidQueueStageImpTests()
    {
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        service = new InsertAutoVoidQueueStageImp(autoVoidRequestQueueService);
    }

    [Fact]
    public async Task ExecuteForAutoVoidWinning_WhenRequestsEmpty_NotExecuteAnyThing()
    {
        // Arrange

        // Act
        await service.ExecuteForAutoVoidWinning(0, [], default, default);

        // Assert
        await autoVoidRequestQueueService.DidNotReceiveWithAnyArgs()
            .InsertQueue(default, default, default, default, default, default);
    }

    [Fact]
    public async Task ExecuteForAutoVoidWinning_WhenHasAnyRequests_ShouldExecuteInsertQueue()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var broadcastSessionIdCache = new Dictionary<int, string> { { 123, "123" } };
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113, MatchId = 85648 }, new() { RequestId = 224, MatchId = 85648 } };

        // Act
        await service.ExecuteForAutoVoidWinning(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache, []);

        // Assert
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "113,224", ExecuteStatus.New, AutoVoidConstant.QueueActionCal, 85648);
    }

    [Fact]
    public async Task ExecuteForAutoVoidWinning_WhenBroadcastSessionIdCacheNotContainsMatchId_ShouldCreateNewGuid()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var broadcastSessionIdCache = new Dictionary<int, string>();
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113, MatchId = 85648 } };
        var logData = new List<string>();

        // Act
        await service.ExecuteForAutoVoidWinning(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache, logData);

        // Assert
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "113", ExecuteStatus.New, AutoVoidConstant.QueueActionCal, 85648);
        Assert.Single(broadcastSessionIdCache);
        Assert.Contains(85648, broadcastSessionIdCache.Keys);
        Assert.Single(logData);
        Assert.Contains("InsertAutoVoidQueueStage-CAL-[1]: 113 - 85648 -", logData[0]);
    }

    [Fact]
    public async Task ExecuteForAutoVoidWinning_WhenMultipleMatchIds_ShouldExecuteInsertQueueForEachMatch()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var broadcastSessionIdCache = new Dictionary<int, string>();
        var fakeRequests = new List<RequestModel>
        {
            new() { RequestId = 113, MatchId = 85648 },
            new() { RequestId = 224, MatchId = 85648 },
            new() { RequestId = 335, MatchId = 85649 },
            new() { RequestId = 446, MatchId = 85649 }
        };
        var logData = new List<string>();

        // Act
        await service.ExecuteForAutoVoidWinning(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache, logData);

        // Assert
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "113,224", ExecuteStatus.New, AutoVoidConstant.QueueActionCal, 85648);
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "335,446", ExecuteStatus.New, AutoVoidConstant.QueueActionCal, 85649);
        Assert.Equal(2, broadcastSessionIdCache.Count);
        Assert.Equal(2, logData.Count);
    }

    [Fact]
    public async Task ExecuteForAutoVoidWinning_ShouldAddLogDataCorrectly()
    {
        // Arrange
        const int fakeSiteDbGroupId = 5;
        var broadcastSessionIdCache = new Dictionary<int, string>();
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113, MatchId = 85648 } };
        var logData = new List<string>();

        // Act
        await service.ExecuteForAutoVoidWinning(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache, logData);

        // Assert
        Assert.Single(logData);
        var logEntry = logData[0];
        Assert.Contains("InsertAutoVoidQueueStage-CAL-[5]: 113 - 85648 -", logEntry);
    }

    [Fact]
    public async Task ExecuteForAutoVoidAllResultKeyed_WhenRequestsEmpty_NotExecuteAnyThing()
    {
        // Arrange

        // Act
        await service.ExecuteForAutoVoidAllResultKeyed(0, [], default, default);

        // Assert
        await autoVoidRequestQueueService.DidNotReceiveWithAnyArgs()
            .InsertQueue(default, default, default, default, default, default);
    }

    [Fact]
    public async Task ExecuteForAutoVoidAllResultKeyed_WhenHasAnyRequests_ShouldExecuteInsertQueue()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113, MatchId = 763831 }, new() { RequestId = 224, MatchId = 763832 } };
        var broadcastSessionIdCache = new Dictionary<int, string> { { 123, "123" } };
        var logData = new List<string>();

        // Act
        await service.ExecuteForAutoVoidAllResultKeyed(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache, logData);

        // Assert
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "113", ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "224", ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);
        Assert.Equal(2, logData.Count);
    }

    [Fact]
    public async Task ExecuteForAutoVoidAllResultKeyed_WhenBroadcastSessionIdCacheNotContainsRequestId_ShouldCreateNewGuid()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var broadcastSessionIdCache = new Dictionary<int, string>();
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113, MatchId = 763831 } };
        var logData = new List<string>();

        // Act
        await service.ExecuteForAutoVoidAllResultKeyed(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache, logData);

        // Assert
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "113", ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);
        Assert.Single(broadcastSessionIdCache);
        Assert.Contains(113, broadcastSessionIdCache.Keys);
        Assert.Single(logData);
        Assert.Contains("InsertAutoVoidQueueStage-VoidResultKeyed-[1]: 113 - 0 -", logData[0]);
    }

    [Fact]
    public async Task ExecuteForAutoVoidAllResultKeyed_WhenMultipleRequestIds_ShouldExecuteInsertQueueForEachRequest()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var broadcastSessionIdCache = new Dictionary<int, string>();
        var fakeRequests = new List<RequestModel>
        {
            new() { RequestId = 113, MatchId = 763831 },
            new() { RequestId = 224, MatchId = 763832 },
            new() { RequestId = 335, MatchId = 763833 }
        };
        var logData = new List<string>();

        // Act
        await service.ExecuteForAutoVoidAllResultKeyed(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache, logData);

        // Assert
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "113", ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "224", ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "335", ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);
        Assert.Equal(3, broadcastSessionIdCache.Count);
        Assert.Equal(3, logData.Count);
    }

    [Fact]
    public async Task ExecuteForAutoVoidAllResultKeyed_ShouldAddLogDataCorrectly()
    {
        // Arrange
        const int fakeSiteDbGroupId = 5;
        var broadcastSessionIdCache = new Dictionary<int, string>();
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113, MatchId = 763831 } };
        var logData = new List<string>();

        // Act
        await service.ExecuteForAutoVoidAllResultKeyed(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache, logData);

        // Assert
        Assert.Single(logData);
        var logEntry = logData[0];
        Assert.Contains("InsertAutoVoidQueueStage-VoidResultKeyed-[5]: 113 - 0 -", logEntry);
    }

    [Fact]
    public async Task ExecuteForAutoVoidAllResultKeyed_WhenDuplicateRequestIds_ShouldExecuteInsertQueueForEachRequest()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var broadcastSessionIdCache = new Dictionary<int, string>();
        var fakeRequests = new List<RequestModel>
        {
            new() { RequestId = 113, MatchId = 763831 },
            new() { RequestId = 113, MatchId = 763832 } // Duplicate RequestId - should still call InsertQueue twice
        };
        var logData = new List<string>();

        // Act
        await service.ExecuteForAutoVoidAllResultKeyed(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache, logData);

        // Assert
        await autoVoidRequestQueueService.Received(2).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "113", ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);
        Assert.Single(broadcastSessionIdCache);
        Assert.Equal(2, logData.Count);
    }
}