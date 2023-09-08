using NSubstitute;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class AutoVoidInstantProcessStageTests
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly CheckVoidInstantRequestQueueValidStage checkRequestQueueValidStage;
    private readonly CheckTicketAllowToVoidStage checkTicketAllowToVoidStage;
    private readonly GetTicketsStage getTicketsStage;
    private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;
    private readonly LoggingService loggingService;
    private readonly AutoVoidInstantProcessStageImp service;

    public AutoVoidInstantProcessStageTests()
    {
        checkRequestQueueValidStage = Substitute.For<CheckVoidInstantRequestQueueValidStage>();
        getTicketsStage = Substitute.For<GetTicketsStage>();
        checkTicketAllowToVoidStage = Substitute.For<CheckTicketAllowToVoidStage>();
        insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        loggingService = Substitute.For<LoggingService>();
        service = new AutoVoidInstantProcessStageImp(
            checkRequestQueueValidStage,
            getTicketsStage,
            checkTicketAllowToVoidStage,
            insertPreVoidingDataStage,
            autoVoidRequestQueueService,
            loggingService
        );
    }

    [Fact]
    public void Execute_HasCheckQueueResultIsNotValid_Return()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel();
        var logData = new List<string>();
        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((false, 0));

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkRequestQueueValidStage.Received(1).Execute(1, queue);
    }

    [Fact]
    public void Execute_HasGetTicketsResultIsNotContinueFlow_Return()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel();
        var logData = new List<string>();
        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((true, 0));
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((false, null));

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        getTicketsStage.Received(1).Execute(1, queue, logData);
    }

    [Theory]
    [InlineData(IssueTypeConstant.AbnormalAccount)]
    [InlineData(IssueTypeConstant.ReducedOver)]
    public void Execute_HasCheckTicketAllowToVoidStage_ReceivedExpected(int issueTypeId)
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel();
        var logData = new List<string>();
        var tickets = new List<RequestDetailTicketModel>();
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = tickets,
            ParlayTickets = tickets,
            CricketTickets = tickets
        };
        var requestDetailTickets = new List<RequestDetailTicketModel>();
        var checkTicketResult = new CheckStatusTicketOutput
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
        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((true, issueTypeId));
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        checkTicketAllowToVoidStage.Execute(1, tickets,
            tickets, tickets, logData).ReturnsForAnyArgs(checkTicketResult);
        insertPreVoidingDataStage.Execute(1, queue, requestDetailTickets,
                requestDetailTickets, requestDetailTickets, requestDetailTickets, logData)
            .ReturnsForAnyArgs("true");

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkTicketAllowToVoidStage.Received(1).Execute(1, tickets,
            tickets, tickets, logData);
        insertPreVoidingDataStage.Received(1).Execute(1, queue, requestDetailTickets,
            requestDetailTickets, requestDetailTickets, requestDetailTickets, logData);
        autoVoidRequestQueueService.Received(1).UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, "true");
        loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<List<string>>());
    }

    [Fact]
    public void Execute_NoCheckTicketAllowToVoidStage_ReceivedExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel();
        var logData = new List<string>();
        var tickets = new List<RequestDetailTicketModel>();
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = tickets,
            ParlayTickets = tickets,
            CricketTickets = tickets
        };
        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((true, IssueTypeConstant.DangerAccepted));
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        insertPreVoidingDataStage.Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
                Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
                Arg.Any<IEnumerable<RequestDetailTicketModel>>(), logData)
            .ReturnsForAnyArgs("true");

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkTicketAllowToVoidStage.Received(0).Execute(1, tickets,
            tickets, tickets, logData);
        insertPreVoidingDataStage.Received(1).Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), logData);
        autoVoidRequestQueueService.Received(1).UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, "true");
        loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<List<string>>());
    }
}