using NSubstitute;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class AutoVoidWinningProcessStageTests
{
    private readonly CheckTicketValidationStage checkTicketAllowToVoidStage;
    private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly CreateVoidDataXmlStage createVoidDataXmlStage;
    private readonly LoggingService loggingService;
    private readonly AutoVoidWinningBeforeVoidService autoVoidWinningBeforeVoidService;

    private readonly AutoVoidWinningProcessStageImp service;

    public AutoVoidWinningProcessStageTests()
    {
        var reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        autoVoidWinningBeforeVoidService = Substitute.For<AutoVoidWinningBeforeVoidService>();
        checkTicketAllowToVoidStage = Substitute.For<CheckTicketValidationStage>();
        insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        createVoidDataXmlStage = Substitute.For<CreateVoidDataXmlStage>();
        loggingService = Substitute.For<LoggingService>();
        service = new AutoVoidWinningProcessStageImp(
            checkTicketAllowToVoidStage,
            reCalculateTicketsStage,
            insertPreVoidingDataStage,
            autoVoidRequestQueueService,
            createVoidDataXmlStage,
            loggingService,
            autoVoidWinningBeforeVoidService
        );
    }

    [Fact]
    public void Execute_RequestsAllChangeAction_Return()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel();
        var logData = new List<string>();
        autoVoidWinningBeforeVoidService.CheckVoidWinningRequestQueueValid(1, queue).Returns((false, string.Empty));
        autoVoidWinningBeforeVoidService.GetTickets(1, queue, logData).ReturnsForAnyArgs((false, null));

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        autoVoidWinningBeforeVoidService.Received(1).CheckVoidWinningRequestQueueValid(1, queue);
        autoVoidWinningBeforeVoidService.Received(0).GetTickets(1, queue, logData);
    }

    [Fact]
    public void Execute_GetTicketsResultHasIsNotContinueFlow_Return()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel();
        var logData = new List<string>();
        autoVoidWinningBeforeVoidService.CheckVoidWinningRequestQueueValid(1, queue).Returns((true, string.Empty));
        autoVoidWinningBeforeVoidService.GetTickets(1, queue, logData).ReturnsForAnyArgs((false, null));

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        autoVoidWinningBeforeVoidService.Received(1).GetTickets(1, queue, logData);
    }

    [Fact]
    public void Execute_GetTicketsResultHasIsContinueFlow_ReceivedExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel { QueueId = 1, Action = AutoVoidConstant.QueueActionVoidInstant };
        var logData = new List<string>();
        const string voidDataXml = "xml";
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = new List<RequestDetailTicketModel>(),
            ParlayTickets = new List<RequestDetailTicketModel>(),
            CricketTickets = new List<RequestDetailTicketModel>(),
            CancelCOTickets = new List<RequestDetailTicketModel>()
        };
        var checkTicketResult = new CheckTicketValidationOutput
        {
            SingleTicketsResettle = new List<RequestDetailTicketModel>(),
            ParlayTicketsResettle = new List<RequestDetailTicketModel>(),
            CricketTicketsResettle = new List<RequestDetailTicketModel>()
        };
        autoVoidWinningBeforeVoidService.CheckVoidWinningRequestQueueValid(1, queue).Returns((true, string.Empty));
        autoVoidWinningBeforeVoidService.GetTickets(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        checkTicketAllowToVoidStage.ExecuteForVoid(1, default, default, default, default, logData).ReturnsForAnyArgs(checkTicketResult);
        createVoidDataXmlStage.Execute(true, Arg.Any<CheckTicketValidationOutput>()).ReturnsForAnyArgs(voidDataXml);
        insertPreVoidingDataStage.Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData).ReturnsForAnyArgs(string.Empty);

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkTicketAllowToVoidStage.Received(1).ExecuteForVoid(1, outputData.SingleTickets,
            outputData.ParlayTickets, outputData.CricketTickets, outputData.CancelCOTickets, logData);
        insertPreVoidingDataStage.Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData);
        autoVoidRequestQueueService.UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, string.Empty);
        loggingService.Received(1).InfoAppendNewLineAsync(logData);
    }
}