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
        await service.ExecuteForAutoVoidWinningAndResultKeyed(0, new List<RequestModel>(), AutoVoidConstant.QueueActionCal, default, default);

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
        await service.ExecuteForAutoVoidWinningAndResultKeyed(fakeSiteDbGroupId, fakeRequests, AutoVoidConstant.QueueActionCal, broadcastSessionIdCache,
            new List<string>());

        // Assert
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(), "113,224", ExecuteStatus.New, AutoVoidConstant.QueueActionCal, 85648);
    }

    [Fact]
    public async Task ExecuteForAutoVoidAllResultKeyed_WhenHasAnyRequests_ShouldExecuteInsertQueue()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113, MatchId = 763831 }, new() { RequestId = 113, MatchId = 763832 } };
        var broadcastSessionIdCache = new Dictionary<int, string> { { 123, "123" } };

        // Act
        await service.ExecuteForAutoVoidWinningAndResultKeyed(fakeSiteDbGroupId, fakeRequests, AutoVoidConstant.QueueActionCal, broadcastSessionIdCache, new List<string>());

        // Assert
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(),
            "113", ExecuteStatus.New, AutoVoidConstant.QueueActionCal, 763831);
        await autoVoidRequestQueueService.Received(1).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(),
            "113", ExecuteStatus.New, AutoVoidConstant.QueueActionCal, 763832);
    }
}