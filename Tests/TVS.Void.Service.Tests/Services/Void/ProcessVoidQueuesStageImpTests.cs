using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Tests.Services.Void;

public class ProcessVoidQueuesStageImpTests
{
    [Theory]
    [InlineData(ExecuteStatus.Failed)]
    [InlineData(ExecuteStatus.Done)]
    public async Task Execute_WhenQueuesAllDoneOrFailed_ShouldNotProcessVoidQueue(int executeStatus)
    {
        // Arrange
        var fakeQueues = new List<TicketVoidingQueueModel> { new() { StatusId = executeStatus } };
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var netPositionService = Substitute.For<NetPositionService>();
        var service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService, netPositionService);

        // Act
        var actual = await service.Execute(1, fakeQueues);

        // Assert
        Assert.Equal(fakeQueues.Count, actual.Count());
        Assert.Equal(fakeQueues.First().StatusId, actual.First().StatusId);
    }

    [Theory]
    [InlineData(ExecuteStatus.Running)]
    public async Task Execute_WhenQueuesAllRunning_ShouldProcessVoidQueue(int executeStatus)
    {
        // Arrange
        var fakeQueues = new List<TicketVoidingQueueModel> { new() { StatusId = executeStatus, QueueId = 223, VoidingTypeId = VoidingTypeConstant.Single } };
        var fakeProcessQueuesResult = new TicketVoidingQueueModel { VoidingHistoryId = 113, StatusId = 2 };
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var netPositionService = Substitute.For<NetPositionService>();
        ticketVoidingQueueService.ProcessVoidQueue(default, default, true).ReturnsForAnyArgs(fakeProcessQueuesResult);
        var service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService, netPositionService);

        // Act
        var actual = await service.Execute(1, fakeQueues);

        // Assert
        await ticketVoidingQueueService.Received(1).ProcessVoidQueue(Arg.Is<int>(x => x == 1),
            Arg.Is<int>(x => x == fakeQueues.First().QueueId), null);
        Assert.Equal(fakeQueues.Count, actual.Count);
        Assert.Equal(fakeProcessQueuesResult, actual.First());
    }
}