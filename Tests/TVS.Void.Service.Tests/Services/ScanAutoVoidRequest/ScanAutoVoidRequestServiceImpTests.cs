using NSubstitute;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class ScanAutoVoidRequestServiceImpTests
{
    [Fact]
    public async Task Execute_QueueIsNull_Return()
    {
        AutoVoidRequestQueueModel? queue = null;
        const int siteDbGroupId = 0;
        var checkRequestQueueValidStage = Substitute.For<CheckVoidInstantRequestQueueValidStage>();
        var getTicketsStage = Substitute.For<GetTicketsStage>();
        var checkTicketAllowToVoidStage = Substitute.For<CheckTicketAllowToVoidStage>();
        var reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        var insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var loggingService = Substitute.For<LoggingService>();

        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);

        var service = new ScanAutoVoidRequestServiceImp(checkRequestQueueValidStage, getTicketsStage,
            checkTicketAllowToVoidStage, reCalculateTicketsStage, insertPreVoidingDataStage,
            autoVoidRequestQueueService, loggingService);

        await service.Execute(siteDbGroupId);

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
        var checkRequestQueueValidStage = Substitute.For<CheckVoidInstantRequestQueueValidStage>();
        var getTicketsStage = Substitute.For<GetTicketsStage>();
        var checkTicketAllowToVoidStage = Substitute.For<CheckTicketAllowToVoidStage>();
        var reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        var insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var loggingService = Substitute.For<LoggingService>();

        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);
        checkRequestQueueValidStage.Execute(default, default).ReturnsForAnyArgs(true);
        checkTicketAllowToVoidStage.Execute(default, default, default, default)
            .ReturnsForAnyArgs(new CheckStatusTicketOutput());
        getTicketsStage.Execute(default, default, default).ReturnsForAnyArgs((true, new GetTicketsStageOutput()));

        var service = new ScanAutoVoidRequestServiceImp(checkRequestQueueValidStage, getTicketsStage,
            checkTicketAllowToVoidStage, reCalculateTicketsStage, insertPreVoidingDataStage,
            autoVoidRequestQueueService, loggingService);

        await service.Execute(siteDbGroupId);

        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
        await checkRequestQueueValidStage.Received(1).Execute(Arg.Any<int>(), Arg.Any<AutoVoidRequestQueueModel>());
        await loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task Execute_IsQueueAutoVoidInstantAndRequestQueueInvalid_CalledExpected()
    {
        var queue = new AutoVoidRequestQueueModel
        {
            Action = AutoVoidConstant.QueueActionVoidInstant
        };
        const int siteDbGroupId = 0;
        var checkRequestQueueValidStage = Substitute.For<CheckVoidInstantRequestQueueValidStage>();
        var getTicketsStage = Substitute.For<GetTicketsStage>();
        var checkTicketAllowToVoidStage = Substitute.For<CheckTicketAllowToVoidStage>();
        var reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        var insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var loggingService = Substitute.For<LoggingService>();

        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);
        checkRequestQueueValidStage.Execute(default, default).ReturnsForAnyArgs(false);
        checkTicketAllowToVoidStage.Execute(default, default, default, default)
            .ReturnsForAnyArgs(new CheckStatusTicketOutput());
        getTicketsStage.Execute(default, default, default).ReturnsForAnyArgs((true, new GetTicketsStageOutput()));

        var service = new ScanAutoVoidRequestServiceImp(checkRequestQueueValidStage, getTicketsStage,
            checkTicketAllowToVoidStage, reCalculateTicketsStage, insertPreVoidingDataStage,
            autoVoidRequestQueueService, loggingService);

        await service.Execute(siteDbGroupId);

        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
        await checkRequestQueueValidStage.Received(1).Execute(Arg.Any<int>(), Arg.Any<AutoVoidRequestQueueModel>());
        await getTicketsStage.Received(0).Execute(Arg.Any<int>(), Arg.Any<AutoVoidRequestQueueModel>(), Arg.Any<IList<string>>());
        await loggingService.Received(0).InfoAppendNewLineAsync(Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task Execute_IsQueueAutoVoidInstantAndIsContinueFlow_CalledExpected()
    {
        var queue = new AutoVoidRequestQueueModel
        {
            Action = AutoVoidConstant.QueueActionVoidInstant
        };
        const int siteDbGroupId = 0;
        var checkRequestQueueValidStage = Substitute.For<CheckVoidInstantRequestQueueValidStage>();
        var getTicketsStage = Substitute.For<GetTicketsStage>();
        var checkTicketAllowToVoidStage = Substitute.For<CheckTicketAllowToVoidStage>();
        var reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        var insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var loggingService = Substitute.For<LoggingService>();

        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);
        checkRequestQueueValidStage.Execute(default, default).ReturnsForAnyArgs(true);
        checkTicketAllowToVoidStage.Execute(default, default, default, default)
            .ReturnsForAnyArgs(new CheckStatusTicketOutput());
        getTicketsStage.Execute(default, default, default).ReturnsForAnyArgs((false, new GetTicketsStageOutput()));

        var service = new ScanAutoVoidRequestServiceImp(checkRequestQueueValidStage, getTicketsStage,
            checkTicketAllowToVoidStage, reCalculateTicketsStage, insertPreVoidingDataStage,
            autoVoidRequestQueueService, loggingService);

        await service.Execute(siteDbGroupId);

        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
        await checkRequestQueueValidStage.Received(1).Execute(Arg.Any<int>(), Arg.Any<AutoVoidRequestQueueModel>());
        await getTicketsStage.Received(1).Execute(Arg.Any<int>(), Arg.Any<AutoVoidRequestQueueModel>(), Arg.Any<IList<string>>());
        await loggingService.Received(0).InfoAppendNewLineAsync(Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task Execute_IsQueueAutoWinning_CalledExpected()
    {
        var queue = new AutoVoidRequestQueueModel { Action = "" };
        const int siteDbGroupId = 0;
        var checkRequestQueueValidStage = Substitute.For<CheckVoidInstantRequestQueueValidStage>();
        var getTicketsStage = Substitute.For<GetTicketsStage>();
        var checkTicketAllowToVoidStage = Substitute.For<CheckTicketAllowToVoidStage>();
        var reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        var insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var loggingService = Substitute.For<LoggingService>();

        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);
        checkTicketAllowToVoidStage.Execute(default, default, default, default)
            .ReturnsForAnyArgs(new CheckStatusTicketOutput());
        getTicketsStage.Execute(default, default, default).ReturnsForAnyArgs((true, new GetTicketsStageOutput()));

        var service = new ScanAutoVoidRequestServiceImp(checkRequestQueueValidStage, getTicketsStage,
            checkTicketAllowToVoidStage, reCalculateTicketsStage, insertPreVoidingDataStage,
            autoVoidRequestQueueService, loggingService);

        await service.Execute(siteDbGroupId);

        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
        await checkTicketAllowToVoidStage.Received(1).Execute(Arg.Any<int>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IList<string>>());
        await loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task Execute_IsQueueAutoWinningAndIsContinueFlow_CalledExpected()
    {
        var queue = new AutoVoidRequestQueueModel { Action = "" };
        const int siteDbGroupId = 0;
        var checkRequestQueueValidStage = Substitute.For<CheckVoidInstantRequestQueueValidStage>();
        var getTicketsStage = Substitute.For<GetTicketsStage>();
        var checkTicketAllowToVoidStage = Substitute.For<CheckTicketAllowToVoidStage>();
        var reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        var insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var loggingService = Substitute.For<LoggingService>();

        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);
        getTicketsStage.Execute(default, default, default).ReturnsForAnyArgs((false, new GetTicketsStageOutput()));

        var service = new ScanAutoVoidRequestServiceImp(checkRequestQueueValidStage, getTicketsStage,
            checkTicketAllowToVoidStage, reCalculateTicketsStage, insertPreVoidingDataStage,
            autoVoidRequestQueueService, loggingService);

        await service.Execute(siteDbGroupId);

        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
        await loggingService.Received(0).InfoAppendNewLineAsync(Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task Execute_HasException_CalledExpected()
    {
        var queue = new AutoVoidRequestQueueModel
        {
            Action = AutoVoidConstant.QueueActionVoidInstant
        };
        const int siteDbGroupId = 0;
        var checkRequestQueueValidStage = Substitute.For<CheckVoidInstantRequestQueueValidStage>();
        var getTicketsStage = Substitute.For<GetTicketsStage>();
        var checkTicketAllowToVoidStage = Substitute.For<CheckTicketAllowToVoidStage>();
        var reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        var insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var loggingService = Substitute.For<LoggingService>();

        autoVoidRequestQueueService.GetQueue(default).ReturnsForAnyArgs(queue);
        checkRequestQueueValidStage.When(x => x.Execute(Arg.Any<int>(), Arg.Any<AutoVoidRequestQueueModel>()))
            .Do(x => throw new Exception());

        var service = new ScanAutoVoidRequestServiceImp(checkRequestQueueValidStage, getTicketsStage,
            checkTicketAllowToVoidStage, reCalculateTicketsStage, insertPreVoidingDataStage,
            autoVoidRequestQueueService, loggingService);

        await service.Execute(siteDbGroupId);

        await autoVoidRequestQueueService.Received(1).GetQueue(Arg.Any<int>());
        await checkRequestQueueValidStage.Received(1).Execute(Arg.Any<int>(), Arg.Any<AutoVoidRequestQueueModel>());
        await loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<IList<string>>());
        await loggingService.Received(1).ErrorAsync(Arg.Any<string>(), Arg.Any<Exception>());
    }
}