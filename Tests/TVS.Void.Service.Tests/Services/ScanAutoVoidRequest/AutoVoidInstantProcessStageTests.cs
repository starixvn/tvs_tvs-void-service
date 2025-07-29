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
        const string voidDataXml = "xml123";
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = new List<RequestDetailTicketModel> { new() },
            ParlayTickets = [],
            CricketTickets = [],
            CancelCOTickets = []
        };
        var validationResult = new CheckTicketValidationOutput();
        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((true, issueTypeId));
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        checkTicketValidationStage.ExecuteForVoid(1, outputData.SingleTickets, outputData.ParlayTickets, outputData.CricketTickets, outputData.CancelCOTickets, logData)
            .Returns(validationResult);
        createVoidDataXmlStage.Execute(true, Arg.Any<CheckTicketValidationOutput>()).ReturnsForAnyArgs(voidDataXml);
        insertPreVoidingDataStage.Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData).ReturnsForAnyArgs(string.Empty);

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        insertPreVoidingDataStage.Received(1).MapVoidingTypeToTickets(validationResult);
        createVoidDataXmlStage.Received(1).Execute(true, validationResult);
        autoVoidRequestQueueService.Received(1).UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, string.Empty);
    }

    [Fact]
    public void Execute_NoCheckTicketValidation_ReceivedExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel { Action = AutoVoidConstant.QueueActionVoidInstant };
        var logData = new List<string>();
        const string voidDataXml = "xml123";
        var getTicketsResult = new GroupTicketsInRequest
        {
            SingleTickets = new List<RequestDetailTicketModel> { new() },
            ParlayTickets = [],
            CricketTickets = [],
            CancelCOTickets = []
        };

        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((true, IssueTypeConstant.DangerAccepted));
        insertPreVoidingDataStage.MapVoidingTypeToTickets(Arg.Any<CheckTicketValidationOutput>()).Returns([]);
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, getTicketsResult));
        createVoidDataXmlStage.Execute(true, Arg.Any<CheckTicketValidationOutput>()).Returns(voidDataXml);
        insertPreVoidingDataStage.Execute(1, queue, [], voidDataXml, logData).Returns(string.Empty);

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        insertPreVoidingDataStage.Received(1).MapVoidingTypeToTickets(Arg.Any<CheckTicketValidationOutput>());
        createVoidDataXmlStage.Received(1).Execute(true, Arg.Any<CheckTicketValidationOutput>());
        autoVoidRequestQueueService.Received(1).UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, string.Empty);
    }

    [Fact]
    public void Execute_HasActionIsRecover_ReceivedExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel { Action = AutoVoidConstant.QueueRecoverInstant };
        var logData = new List<string>();
        const string voidDataXml = "xml123";
        var outputData = new GroupTicketsInRequest
        {
            SingleTickets = new List<RequestDetailTicketModel> { new() },
            ParlayTickets = [],
            CricketTickets = [],
            CancelCOTickets = []
        };
        const string queueErrorMessage = "error";
        checkRequestQueueValidStage.Execute(1, queue).ReturnsForAnyArgs((true, IssueTypeConstant.DangerAccepted));
        getTicketsStage.Execute(1, queue, logData).ReturnsForAnyArgs((true, outputData));
        createVoidDataXmlStage.Execute(false, Arg.Any<CheckTicketValidationOutput>()).ReturnsForAnyArgs(voidDataXml);
        insertPreVoidingDataStage.Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData).ReturnsForAnyArgs(queueErrorMessage);

        // Act
        _ = service.Execute(1, queue, logData);

        // Assert
        checkTicketValidationStage.Received(0).ExecuteForVoid(1, outputData.SingleTickets, outputData.ParlayTickets, outputData.CricketTickets, outputData.CancelCOTickets, logData);
        checkTicketValidationStage.Received(1).ExecuteForRecover(1, Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(), logData);
        insertPreVoidingDataStage.Received(1).MapVoidingTypeToTickets(Arg.Any<CheckTicketValidationOutput>());
        insertPreVoidingDataStage.Received(1).Execute(1, queue, Arg.Any<IEnumerable<RequestDetailTicketModel>>(), voidDataXml, logData);
        loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<IList<string>>());
        autoVoidRequestQueueService.UpdateQueue(1, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
    }
}