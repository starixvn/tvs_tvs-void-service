using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Tests.Services.Void;

public class VoidServiceImpTests
{
    [Fact]
    public async Task Execute_WhenNotHaveAnyQueues_ShouldReturn()
    {
        // Arrange
        const int fakeSiteDbGroupId = 2;
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var processVoidQueuesStage = Substitute.For<ProcessVoidQueuesStage>();
        ticketVoidingQueueService.GetQueues(default, default).ReturnsForAnyArgs(new List<TicketVoidingQueueModel>());
        var service = new VoidServiceImp(ticketVoidingQueueService, default, processVoidQueuesStage, default,
            default, default);
        // Act
        await service.Execute(fakeSiteDbGroupId);

        // Assert
        await ticketVoidingQueueService.Received(1).GetQueues(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<int?>(x => x == null));
        await processVoidQueuesStage.DidNotReceiveWithAnyArgs().Execute(default, default);
    }

    [Theory]
    [InlineData(ExecuteStatus.Failed)]
    [InlineData(ExecuteStatus.New)]
    public async Task Execute_WhenProcessQueueResultsHasAnyFailed_ShouldExecuteNormalFlow(int executeStatus)
    {
        // Arrange
        const int fakeSiteDbGroupId = 2;
        var fakeGetQueues = new List<TicketVoidingQueueModel> { new() { QueueId = 123 } };
        var fakeProcessResults = new List<TicketVoidingQueueModel> { new() { StatusId = executeStatus } };
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var processVoidQueuesStage = Substitute.For<ProcessVoidQueuesStage>();
        var requestDetailService = Substitute.For<RequestDetailService>();
        var notificationService = Substitute.For<NotificationService>();
        var loggingService = Substitute.For<LoggingService>();
        ticketVoidingQueueService.GetQueues(default, default).ReturnsForAnyArgs(fakeGetQueues);
        processVoidQueuesStage.Execute(default, default).ReturnsForAnyArgs(fakeProcessResults);
        var service = new VoidServiceImp(ticketVoidingQueueService, requestDetailService, processVoidQueuesStage, default,
            notificationService, loggingService);

        // Act
        await service.Execute(fakeSiteDbGroupId);

        // Assert
        await ticketVoidingQueueService.Received(1).GetQueues(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<int?>(x => x == null));
        await processVoidQueuesStage.Received(1).Execute(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<IEnumerable<TicketVoidingQueueModel>>(x => x == fakeGetQueues));
        await requestDetailService.ReceivedWithAnyArgs(1).UpdateTicketVoidingHistory(default, default);
        await notificationService.ReceivedWithAnyArgs(1).NotifyVoidFailedToUser(default, default);
        await notificationService.ReceivedWithAnyArgs(1).NotifyRequestFailedToAllUser(default);
        await loggingService.ReceivedWithAnyArgs(1).InfoAppendNewLineAsync(default);
    }

    [Theory]
    [InlineData(ExecuteStatus.Running)]
    public async Task Execute_WhenProcessQueueResultsHasAnyRunning_ShouldExecuteNormalFlow(int executeStatus)
    {
        // Arrange
        const int fakeSiteDbGroupId = 2;
        var fakeGetQueues = new List<TicketVoidingQueueModel> { new() { QueueId = 123 } };
        var fakeProcessResults = new List<TicketVoidingQueueModel> { new() { StatusId = executeStatus } };
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var processVoidQueuesStage = Substitute.For<ProcessVoidQueuesStage>();
        var requestDetailService = Substitute.For<RequestDetailService>();
        var notificationService = Substitute.For<NotificationService>();
        var loggingService = Substitute.For<LoggingService>();
        ticketVoidingQueueService.GetQueues(default, default).ReturnsForAnyArgs(fakeGetQueues);
        processVoidQueuesStage.Execute(default, default).ReturnsForAnyArgs(fakeProcessResults);
        var service = new VoidServiceImp(ticketVoidingQueueService, requestDetailService, processVoidQueuesStage, default,
            notificationService, loggingService);

        // Act
        await service.Execute(fakeSiteDbGroupId);

        // Assert
        await ticketVoidingQueueService.Received(1).GetQueues(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<int?>(x => x == null));
        await processVoidQueuesStage.Received(1).Execute(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<IEnumerable<TicketVoidingQueueModel>>(x => x == fakeGetQueues));
        await requestDetailService.DidNotReceiveWithAnyArgs().UpdateTicketVoidingHistory(default, default);
        await notificationService.DidNotReceiveWithAnyArgs().NotifyVoidFailedToUser(default, default);
        await notificationService.DidNotReceiveWithAnyArgs().NotifyRequestFailedToAllUser(default);
        await loggingService.ReceivedWithAnyArgs(1).InfoAppendNewLineAsync(default);
    }

    [Theory]
    [InlineData(ExecuteStatus.Done)]
    public async Task Execute_WhenProcessQueueResultsAllDone_ShouldExecuteInsertAfterVoidData(int executeStatus)
    {
        // Arrange
        const int fakeSiteDbGroupId = 2;
        var fakeGetQueues = new List<TicketVoidingQueueModel> { new() { QueueId = 123 } };
        var fakeProcessResults = new List<TicketVoidingQueueModel> { new() { StatusId = executeStatus } };
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var processVoidQueuesStage = Substitute.For<ProcessVoidQueuesStage>();
        var requestDetailService = Substitute.For<RequestDetailService>();
        var notificationService = Substitute.For<NotificationService>();
        var insertAfterVoidDataStage = Substitute.For<InsertAfterVoidDataStage>();
        var loggingService = Substitute.For<LoggingService>();
        ticketVoidingQueueService.GetQueues(default, default).ReturnsForAnyArgs(fakeGetQueues);
        processVoidQueuesStage.Execute(default, default).ReturnsForAnyArgs(fakeProcessResults);
        var service = new VoidServiceImp(ticketVoidingQueueService, requestDetailService, processVoidQueuesStage, insertAfterVoidDataStage,
            notificationService, loggingService);

        // Act
        await service.Execute(fakeSiteDbGroupId);

        // Assert
        await ticketVoidingQueueService.Received(1).GetQueues(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<int?>(x => x == null));
        await processVoidQueuesStage.Received(1).Execute(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<IEnumerable<TicketVoidingQueueModel>>(x => x == fakeGetQueues));
        await requestDetailService.DidNotReceiveWithAnyArgs().UpdateTicketVoidingHistory(default, default);
        await notificationService.DidNotReceiveWithAnyArgs().NotifyVoidFailedToUser(default, default);
        await notificationService.DidNotReceiveWithAnyArgs().NotifyRequestFailedToAllUser(default);
        await insertAfterVoidDataStage.Received(1).Execute(Arg.Is<IEnumerable<TicketVoidingQueueModel>>(x => x == fakeProcessResults),
            Arg.Any<IList<string>>());
        await loggingService.ReceivedWithAnyArgs(1).InfoAppendNewLineAsync(default);
    }

    [Fact]
    public async Task Execute_WhenExecuteThrowException_ShouldRunHandleExceptionFlow()
    {
        // Arrange
        const int fakeSiteDbGroupId = 2;
        var fakeGetQueues = new List<TicketVoidingQueueModel> { new() { QueueId = 123 } };
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var loggingService = Substitute.For<LoggingService>();
        ticketVoidingQueueService.GetQueues(default, default).ReturnsForAnyArgs(fakeGetQueues);

        var service = new VoidServiceImp(ticketVoidingQueueService, default, default, default,
            default, loggingService);

        // Act
        await service.Execute(fakeSiteDbGroupId);

        // Assert
        await ticketVoidingQueueService.Received(1).GetQueues(Arg.Is<int>(x => x == fakeSiteDbGroupId), Arg.Is<int?>(x => x == null));
        await loggingService.ReceivedWithAnyArgs(1).InfoAppendNewLineAsync(default);
        await loggingService.ReceivedWithAnyArgs(1).ErrorAsync(default, default);
    }
}