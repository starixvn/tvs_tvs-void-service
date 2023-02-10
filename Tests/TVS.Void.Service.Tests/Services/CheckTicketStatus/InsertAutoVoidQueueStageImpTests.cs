using NSubstitute;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.CheckTicketStatus;

namespace TVS.Void.Service.Tests.Services.CheckTicketStatus;

public class InsertAutoVoidQueueStageImpTests
{
    [Fact]
    public async Task Execute_WhenRequestsEmpty_NotExecuteAnyThing()
    {
        // Arrange
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var stage = new InsertAutoVoidQueueStageImp(autoVoidRequestQueueService);

        // Act
        await stage.Execute(0, new List<RequestModel>(), default, default);

        // Assert
        await autoVoidRequestQueueService.DidNotReceiveWithAnyArgs()
            .InsertQueue(default, default, default, default, default, default);
    }

    [Fact]
    public async Task Execute_WhenRequestsNull_ThrowException()
    {
        // Arrange
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var stage = new InsertAutoVoidQueueStageImp(autoVoidRequestQueueService);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => stage.Execute(0, null, default, default));
    }

    [Fact]
    public async Task Execute_WhenHasAnyRequests_ShouldExecuteInsertQueue()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const string fakeBroadcastSessionId = "123asdad";
        var fakeRequests = new List<RequestModel> { new() { RequestId = 113 }, new() { RequestId = 224 } };
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var stage = new InsertAutoVoidQueueStageImp(autoVoidRequestQueueService);

        // Act
        await stage.Execute(fakeSiteDbGroupId, fakeRequests, fakeBroadcastSessionId, new List<string>());

        // Assert
        await autoVoidRequestQueueService.Received(1)
            .InsertQueue(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<string>(x => x == fakeBroadcastSessionId),
                Arg.Is<string>(x => x == "113,224"), Arg.Any<int>(), Arg.Any<string>(), Arg.Is<int?>(x => x == null));
    }
}