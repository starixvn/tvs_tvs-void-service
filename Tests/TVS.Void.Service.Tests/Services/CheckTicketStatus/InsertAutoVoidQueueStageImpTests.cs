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
        await service.ExecuteForAutoVoidWinning(0, new List<RequestModel>(), default, default);

        // Assert
        await autoVoidRequestQueueService.DidNotReceiveWithAnyArgs()
            .InsertQueue(default, default, default, default, default, default);
    }

    [Fact]
    public async Task ExecuteForAutoVoidWinning_WhenHasAnyRequests_ShouldExecuteInsertQueue()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const string fakeBroadcastSessionId = "123asdad";
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113 }, new() { RequestId = 224 } };

        // Act
        await service.ExecuteForAutoVoidWinning(fakeSiteDbGroupId, fakeRequests, fakeBroadcastSessionId,
            new List<string>());

        // Assert
        await autoVoidRequestQueueService.Received(1)
            .InsertQueue(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<string>(x => x == fakeBroadcastSessionId),
                Arg.Is<string>(x => x == "113,224"), Arg.Any<int>(), Arg.Any<string>(), Arg.Is<int?>(x => x == null));
    }

    [Fact]
    public async Task ExecuteForAutoVoidAllResultKeyed_WhenHasAnyRequests_ShouldExecuteInsertQueue()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113 }, new() { RequestId = 224 } };
        var broadcastSessionIdCache = new Dictionary<int, string> { { 123, "123" } };

        // Act
        await service.ExecuteForAutoVoidAllResultKeyed(fakeSiteDbGroupId, fakeRequests, broadcastSessionIdCache,
            new List<string>());

        // Assert
        await autoVoidRequestQueueService.Received(2).InsertQueue(fakeSiteDbGroupId, Arg.Any<string>(),
            Arg.Any<string>(),
            ExecuteStatus.New, AutoVoidConstant.QueueActionVoidResultKeyed, null);
    }
}