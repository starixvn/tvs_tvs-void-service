using NSubstitute;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class AutoVoidInstantProcessStageTests
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly CheckVoidInstantRequestQueueValidStage checkRequestQueueValidStage;
    private readonly CheckTicketValidationStage checkTicketValidationStage;
    private readonly GetTicketsStage getTicketsStage;
    private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;
    private readonly LoggingService loggingService;
    private readonly CreateVoidDataXmlStage createVoidDataXmlStage;
    private readonly AutoInstantProcessStageImp service;

    public AutoVoidInstantProcessStageTests()
    {
        checkRequestQueueValidStage = Substitute.For<CheckVoidInstantRequestQueueValidStage>();
        getTicketsStage = Substitute.For<GetTicketsStage>();
        checkTicketValidationStage = Substitute.For<CheckTicketValidationStage>();
        insertPreVoidingDataStage = Substitute.For<InsertPreVoidingDataStage>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        createVoidDataXmlStage = Substitute.For<CreateVoidDataXmlStage>();
        loggingService = Substitute.For<LoggingService>();
        service = new AutoInstantProcessStageImp(
            checkRequestQueueValidStage,
            getTicketsStage,
            checkTicketValidationStage,
            insertPreVoidingDataStage,
            autoVoidRequestQueueService,
            createVoidDataXmlStage,
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
        var queue = new AutoVoidRequestQueueModel { Action = AutoVoidConstant.QueueActionVoidInstant };
        var logData = new List<string>();
        var tickets = new List<RequestDetailTicketModel>();
        var voidDataXml = "xml123";
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = tickets,
            ParlayTickets = tickets,
            CricketTickets = tickets,
            CancelCOTickets = tickets
        };
        const string queueErrorMessage = "error";
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
        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((true, issueTypeId));
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        checkTicketValidationStage.ExecuteForVoid(1, tickets, tickets, tickets, tickets, logData).ReturnsForAnyArgs(checkTicketResult);
        createVoidDataXmlStage.Execute(Arg.Any<bool>(), Arg.Any<CheckTicketValidationOutput>())
            .ReturnsForAnyArgs(voidDataXml);
        insertPreVoidingDataStage.Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData).ReturnsForAnyArgs(queueErrorMessage);

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkTicketValidationStage.Received(1).ExecuteForVoid(1, tickets, tickets, tickets, tickets, logData);
        insertPreVoidingDataStage.Received(1).MapVoidingTypeToTickets(Arg.Any<CheckTicketValidationOutput>());
        createVoidDataXmlStage.Received(1).Execute(Arg.Any<bool>(), Arg.Any<CheckTicketValidationOutput>());
        insertPreVoidingDataStage.Received(1).Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData);
        autoVoidRequestQueueService.UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
    }

    [Fact]
    public void Execute_NoCheckTicketValidation_ReceivedExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel { Action = AutoVoidConstant.QueueActionVoidInstant };
        var logData = new List<string>();
        var tickets = new List<RequestDetailTicketModel>();
        var voidDataXml = "xml123";
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = tickets,
            ParlayTickets = tickets,
            CricketTickets = tickets
        };
        const string queueErrorMessage = "error";
        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((true, IssueTypeConstant.DangerAccepted));
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        createVoidDataXmlStage.Execute(Arg.Any<bool>(), Arg.Any<CheckTicketValidationOutput>())
            .ReturnsForAnyArgs(voidDataXml);
        insertPreVoidingDataStage.Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData).ReturnsForAnyArgs(queueErrorMessage);

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkTicketValidationStage.Received(0).ExecuteForVoid(1, tickets, tickets, tickets, tickets, logData);
        checkTicketValidationStage.Received(0).ExecuteForRecover(1, Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(), logData);
        insertPreVoidingDataStage.Received(1).MapVoidingTypeToTickets(Arg.Any<CheckTicketValidationOutput>());
        createVoidDataXmlStage.Received(1).Execute(Arg.Any<bool>(), Arg.Any<CheckTicketValidationOutput>());
        insertPreVoidingDataStage.Received(1).Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData);
        autoVoidRequestQueueService.UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
    }

    [Fact]
    public void Execute_HasActionIsRecover_ReceivedExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel { Action = AutoVoidConstant.QueueRecoverInstant };
        var logData = new List<string>();
        var tickets = new List<RequestDetailTicketModel>();
        var voidDataXml = "xml123";
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = tickets,
            ParlayTickets = tickets,
            CricketTickets = tickets
        };
        const string queueErrorMessage = "error";
        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((true, IssueTypeConstant.DangerAccepted));
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        createVoidDataXmlStage.Execute(Arg.Any<bool>(), Arg.Any<CheckTicketValidationOutput>())
            .ReturnsForAnyArgs(voidDataXml);
        insertPreVoidingDataStage.Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData).ReturnsForAnyArgs(queueErrorMessage);

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkTicketValidationStage.Received(0).ExecuteForVoid(1, tickets, tickets, tickets, tickets, logData);
        checkTicketValidationStage.Received(1).ExecuteForRecover(1, Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(), logData);
        insertPreVoidingDataStage.Received(1).MapVoidingTypeToTickets(Arg.Any<CheckTicketValidationOutput>());
        createVoidDataXmlStage.Received(1).Execute(Arg.Any<bool>(), Arg.Any<CheckTicketValidationOutput>());
        insertPreVoidingDataStage.Received(1).Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData);
        autoVoidRequestQueueService.UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
    }
}