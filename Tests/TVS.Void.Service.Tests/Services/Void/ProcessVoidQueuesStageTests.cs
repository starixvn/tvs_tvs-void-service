using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Tests.Services.Void;

public class ProcessVoidQueuesStageTests
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
    [InlineData(ExecuteStatus.Running, true)]
    [InlineData(ExecuteStatus.Running, false)]
    [InlineData(ExecuteStatus.Failed, true)]
    public async Task Execute_WhenQueuesAllRunning_ShouldProcessVoidQueue(int executeStatus, bool isCalledNetPosition)
    {
        // Arrange
        var fakeQueues = new List<TicketVoidingQueueModel>
        {
            new()
            {
                TransIds = "1", StatusId = ExecuteStatus.Running, QueueId = 223,
                VoidingTypeId = VoidingTypeConstant.Cricket
            }
        };
        var fakeProcessQueuesResult = new TicketVoidingQueueModel { VoidingHistoryId = 113, StatusId = executeStatus };
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var netPositionService = Substitute.For<NetPositionService>();
        ticketVoidingQueueService.ProcessVoidQueue(default, default, default)
            .ReturnsForAnyArgs(fakeProcessQueuesResult);
        var service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService, netPositionService);
        netPositionService.UpdateNetPosition(default, Arg.Any<string>(), "")
            .ReturnsForAnyArgs(isCalledNetPosition);

        // Act
        var actual = await service.Execute(1, fakeQueues);

        // Assert
        await ticketVoidingQueueService.Received(1).ProcessVoidQueue(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>());
        Assert.Equal(fakeQueues.Count, actual.Count);
        Assert.Equal(fakeProcessQueuesResult, actual.First());
    }

    [Fact]
    public async Task Execute_HasVoidingTypeIdIsNotCricket_CalledExpected()
    {
        // Arrange
        var fakeQueues = new List<TicketVoidingQueueModel>
        {
            new()
            {
                TransIds = "1", StatusId = ExecuteStatus.Running, QueueId = 223,
                VoidingTypeId = VoidingTypeConstant.Single
            }
        };
        var fakeProcessQueuesResult = new TicketVoidingQueueModel
            { VoidingHistoryId = 113, StatusId = ExecuteStatus.Running };
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var netPositionService = Substitute.For<NetPositionService>();
        ticketVoidingQueueService.ProcessVoidQueue(default, default, default)
            .ReturnsForAnyArgs(fakeProcessQueuesResult);
        var service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService, netPositionService);
        netPositionService.UpdateNetPosition(default, Arg.Any<string>(), "").ReturnsForAnyArgs(true);

        // Act
        _ = await service.Execute(1, fakeQueues);

        // Assert
        await ticketVoidingQueueService.Received(1).ProcessVoidQueue(Arg.Any<int>(), Arg.Any<int>(), null);
    }
}