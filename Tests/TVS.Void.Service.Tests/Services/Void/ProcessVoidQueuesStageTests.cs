using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Tests.Services.Void;

public class ProcessVoidQueuesStageTests
{
    private TicketVoidingQueueService ticketVoidingQueueService = null!;
    private NetPositionService netPositionService = null!;
    private ProcessVoidQueuesStageImp service = null!;

    internal void InItService()
    {
        ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        netPositionService = Substitute.For<NetPositionService>();
    }

    [Theory]
    [InlineData(ExecuteStatus.Failed)]
    [InlineData(ExecuteStatus.Done)]
    public async Task Execute_WhenQueuesAllDoneOrFailed_ShouldNotProcessVoidQueue(int executeStatus)
    {
        // Arrange
        var fakeQueues = new List<TicketVoidingQueueModel> { new() { StatusId = executeStatus } };
        InItService();
        service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService, netPositionService);

        // Act
        var actual = await service.Execute(1, fakeQueues);

        // Assert
        Assert.Equal(fakeQueues.Count, actual.Count);
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
        InItService();
        ticketVoidingQueueService.ProcessVoidQueue(default, default, default)
            .ReturnsForAnyArgs(fakeProcessQueuesResult);
        netPositionService.UpdateNetPosition(default, Arg.Any<string>(), "")
            .ReturnsForAnyArgs(isCalledNetPosition);
        service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService, netPositionService);

        // Act
        var actual = await service.Execute(1, fakeQueues);

        // Assert
        await ticketVoidingQueueService.Received(1).ProcessVoidQueue(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>());
        Assert.Equal(fakeQueues.Count, actual.Count);
        Assert.Equal(fakeProcessQueuesResult, actual.First());
    }

    [Fact]
    public async Task Execute_HasVoidingTypeIdIsSingle_CalledExpected()
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
        InItService();
        ticketVoidingQueueService.ProcessVoidQueue(default, default, default)
            .ReturnsForAnyArgs(fakeProcessQueuesResult);
        service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService, netPositionService);
        netPositionService.UpdateNetPosition(default, Arg.Any<string>(), "").ReturnsForAnyArgs(true);

        // Act
        _ = await service.Execute(1, fakeQueues);

        // Assert
        await ticketVoidingQueueService.Received(1).ProcessVoidQueue(Arg.Any<int>(), Arg.Any<int>(), null);
    }

    [Fact]
    public async Task Execute_HasVoidingTypeIdIsRecoverCricket_CalledExpected()
    {
        // Arrange
        var fakeQueues = new List<TicketVoidingQueueModel>
        {
            new()
            {
                TransIds = "1", StatusId = ExecuteStatus.Running, QueueId = 223,
                VoidingTypeId = VoidingTypeConstant.RecoverCricket
            }
        };

        var fakeProcessQueuesResult = new TicketVoidingQueueModel { VoidingHistoryId = 113, StatusId = ExecuteStatus.Running, ResettledTransIds = "1" };
        InItService();
        ticketVoidingQueueService.ProcessVoidQueue(default, default, default)
            .ReturnsForAnyArgs(fakeProcessQueuesResult);
        service = new ProcessVoidQueuesStageImp(ticketVoidingQueueService, netPositionService);
        netPositionService.UpdateNetPosition(default, Arg.Any<string>(), "").ReturnsForAnyArgs(true);

        // Act
        _ = await service.Execute(1, fakeQueues);

        // Assert
        await netPositionService.Received(1).UpdateNetPosition(1, "1", NetPositionConstant.ActionRecover);
    }
}