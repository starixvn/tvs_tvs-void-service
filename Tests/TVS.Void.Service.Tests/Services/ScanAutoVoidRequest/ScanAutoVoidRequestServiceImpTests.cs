using NSubstitute;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class ScanAutoVoidRequestServiceImpTests
{

    private readonly AutoVoidWinningProcessStage autoVoidWinningProcessStage;
    private readonly AutoInstantProcessStage autoVoidInstantProcessStage;
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly LoggingService loggingService;
    private ScanAutoVoidRequestServiceImp service;

    public ScanAutoVoidRequestServiceImpTests()
    {
        autoVoidWinningProcessStage = Substitute.For<AutoVoidWinningProcessStage>();
        autoVoidInstantProcessStage = Substitute.For<AutoInstantProcessStage>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        loggingService = Substitute.For<LoggingService>();
        service = new ScanAutoVoidRequestServiceImp(autoVoidWinningProcessStage, autoVoidInstantProcessStage, autoVoidRequestQueueService, loggingService);
    }

    [Fact]
    public async Task Execute_QueueIsNull_Return()
    {   // Arrange
        AutoVoidRequestQueueModel? queue = null;
        const int siteDbGroupId = 0;
        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);

        // Act
        await service.Execute(siteDbGroupId);

        // Assert
        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
    }

    [Theory]
    [InlineData(AutoVoidConstant.QueueActionVoidInstant)]
    [InlineData(AutoVoidConstant.QueueActionVoidInstant5m)]
    [InlineData(AutoVoidConstant.QueueActionVoidInstant10m)]
    [InlineData(AutoVoidConstant.QueueActionVoidInstantNoPolicy)]
    public async Task Execute_IsQueueAutoVoidInstant_CalledExpected(string action)
    {
        var queue = new AutoVoidRequestQueueModel
        {
            Action = action
        };
        const int siteDbGroupId = 0;
        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);

        await service.Execute(siteDbGroupId);

        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
        await autoVoidInstantProcessStage.Received(1)
            .Execute(default, Arg.Any<AutoVoidRequestQueueModel>(), Arg.Any<List<string>>());
    }

    [Fact]
    public async Task Execute_IsNotQueueAutoVoidInstant_CalledExpected()
    {
        var queue = new AutoVoidRequestQueueModel
        {
            Action = AutoVoidConstant.ActorName
        };
        const int siteDbGroupId = 0;
        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);

        await service.Execute(siteDbGroupId);

        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
        await autoVoidWinningProcessStage.Received(1)
            .Execute(default, Arg.Any<AutoVoidRequestQueueModel>(), Arg.Any<List<string>>());
    }

    [Fact]
    public async Task Execute_HasException_CalledExpected()
    {
        var queue = new AutoVoidRequestQueueModel
        {
            Action = AutoVoidConstant.ActorName
        };
        const int siteDbGroupId = 0;
        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);
        service = new ScanAutoVoidRequestServiceImp(null, autoVoidInstantProcessStage, autoVoidRequestQueueService, loggingService);

        await service.Execute(siteDbGroupId);

        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
        await loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<List<string>>());
        await loggingService.ErrorAsync(Arg.Any<string>(), Arg.Any<Exception>());
        await autoVoidRequestQueueService.UpdateQueue(Arg.Any<int>(), Arg.Any<int>(), ExecuteStatus.Failed, null);
    }
}