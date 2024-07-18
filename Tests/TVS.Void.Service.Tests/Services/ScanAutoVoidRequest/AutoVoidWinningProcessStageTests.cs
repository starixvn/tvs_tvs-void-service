using NSubstitute;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class AutoVoidWinningProcessStageTests
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly CheckTicketValidationStage checkTicketAllowToVoidStage;
    private readonly GetTicketsStage getTicketsStage;
    private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;
    private readonly LoggingService loggingService;
    private readonly ReCalculateTicketsStage reCalculateTicketsStage;
    private readonly CreateVoidDataXmlStage createVoidDataXmlStage;
    private readonly AutoVoidWinningProcessStageImp service;

    public AutoVoidWinningProcessStageTests()
    {
        reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        getTicketsStage = Substitute.For<GetTicketsStage>();
        checkTicketAllowToVoidStage = Substitute.For<CheckTicketValidationStage>();
        insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        createVoidDataXmlStage = Substitute.For<CreateVoidDataXmlStage>();
        loggingService = Substitute.For<LoggingService>();
        service = new AutoVoidWinningProcessStageImp(
            getTicketsStage,
            checkTicketAllowToVoidStage,
            reCalculateTicketsStage,
            insertPreVoidingDataStage,
            autoVoidRequestQueueService,
            createVoidDataXmlStage,
            loggingService
        );
    }

    [Fact]
    public void Execute_GetTicketsResultHasIsNotContinueFlow_Return()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel();
        var logData = new List<string>();
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((false, null));

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        getTicketsStage.Received(1).Execute(1, queue, logData);
    }

    [Fact]
    public void Execute_GetTicketsResultHasIsContinueFlow_ReceivedExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel { QueueId = 1 };
        var logData = new List<string>();

        var voidDataXml = "xml";
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = new List<RequestDetailTicketModel>(),
            ParlayTickets = new List<RequestDetailTicketModel>(),
            CricketTickets = new List<RequestDetailTicketModel>(),
            CancelCOTickets = new List<RequestDetailTicketModel>()
        };
        var requestDetailTickets = new List<RequestDetailTicketModel>();
        var checkTicketResult = new CheckTicketValidationOutput
        {
            VoidSingleTickets = requestDetailTickets,
            SingleTicketsResettle = requestDetailTickets,
            VoidParlayTickets = requestDetailTickets,
            ParlayTicketsResettle = requestDetailTickets,
            VoidCricketTickets = requestDetailTickets,
            CricketTicketsResettle = requestDetailTickets,
            VoidSingleTickets14 = requestDetailTickets,
            VoidParlayTickets14 = requestDetailTickets,
            RecoverSingleTickets = requestDetailTickets,
            RecoverParlayTickets = requestDetailTickets,
            RecoverCricketTickets = requestDetailTickets,
            RecoverParlayTickets14 = requestDetailTickets,
            RecoverSingleTickets14 = requestDetailTickets
        };
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        checkTicketAllowToVoidStage.ExecuteForVoid(1, default,
            default, default, default, logData).ReturnsForAnyArgs(checkTicketResult);
        createVoidDataXmlStage.Execute(Arg.Any<bool>(), Arg.Any<CheckTicketValidationOutput>())
            .ReturnsForAnyArgs(voidDataXml);
        insertPreVoidingDataStage.Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData).ReturnsForAnyArgs("error");

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkTicketAllowToVoidStage.Received(1).ExecuteForVoid(1, outputData.SingleTickets,
            outputData.ParlayTickets, outputData.CricketTickets, outputData.CancelCOTickets, logData);
        insertPreVoidingDataStage.Received(1).Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData);
        createVoidDataXmlStage.Received(1).Execute(Arg.Any<bool>(), Arg.Any<CheckTicketValidationOutput>());
        autoVoidRequestQueueService.Received(1).UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, "error");
        loggingService.Received(1).InfoAppendNewLineAsync(logData);
    }
}