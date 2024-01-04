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
    private readonly AutoVoidWinningProcessStageImp service;

    public AutoVoidWinningProcessStageTests()
    {
        reCalculateTicketsStage = Substitute.For<ReCalculateTicketsStage>();
        getTicketsStage = Substitute.For<GetTicketsStage>();
        checkTicketAllowToVoidStage = Substitute.For<CheckTicketValidationStage>();
        insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        loggingService = Substitute.For<LoggingService>();
        service = new AutoVoidWinningProcessStageImp(
            getTicketsStage,
            checkTicketAllowToVoidStage,
            reCalculateTicketsStage,
            insertPreVoidingDataStage,
            autoVoidRequestQueueService,
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
        var tickets = new List<RequestDetailTicketModel>();
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = tickets,
            ParlayTickets = tickets,
            CricketTickets = tickets
        };
        var requestDetailTickets = new List<RequestDetailTicketModel>();
        var checkTicketResult = new CheckTicketValidationOutput
        {
            ValidVoidSingleTickets = requestDetailTickets,
            SingleTicketsResettle = requestDetailTickets,
            ValidVoidParlayTickets = requestDetailTickets,
            ParlayTicketsResettle = requestDetailTickets,
            ValidVoidCricketTickets = requestDetailTickets,
            CricketTicketsResettle = requestDetailTickets,
            ValidCancelCOTickets = requestDetailTickets,
            ValidVoidSingleTickets14 = requestDetailTickets
        };
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        checkTicketAllowToVoidStage.Execute(1, tickets,
            tickets, tickets, logData).ReturnsForAnyArgs(checkTicketResult);
        insertPreVoidingDataStage.Execute(1, queue,
            checkTicketResult.ValidVoidSingleTickets, checkTicketResult.ValidVoidParlayTickets,
            checkTicketResult.ValidVoidCricketTickets,
            Enumerable.Empty<RequestDetailTicketModel>(), logData).ReturnsForAnyArgs("true");

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkTicketAllowToVoidStage.Received(1).Execute(1, tickets,
            tickets, tickets, logData);
        reCalculateTicketsStage.Execute(1, checkTicketResult.SingleTicketsResettle,
            checkTicketResult.ParlayTicketsResettle, checkTicketResult.CricketTicketsResettle, logData);
        insertPreVoidingDataStage.Received(1).Execute(1, queue,
            checkTicketResult.ValidVoidSingleTickets, checkTicketResult.ValidVoidParlayTickets,
            checkTicketResult.ValidVoidCricketTickets,
            Enumerable.Empty<RequestDetailTicketModel>(), logData);
        autoVoidRequestQueueService.Received(1).UpdateQueue(1, 1, ExecuteStatus.Done, "true");
        loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<List<string>>());
    }
}