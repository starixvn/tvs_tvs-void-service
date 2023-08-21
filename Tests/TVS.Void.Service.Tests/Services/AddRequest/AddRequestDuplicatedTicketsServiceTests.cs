using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.AddRequest;

namespace TVS.Void.Service.Tests.Services.AddRequest;

public class AddRequestDuplicatedTicketsServiceTests
{
    private readonly AddRequestStage addRequestStage;
    private readonly CheckDuplicatedTicketsStage checkDuplicatedTicketsStage;
    private readonly GetDuplicatedTicketsStage getDuplicatedTicketsStage;
    private readonly InsertAutoVoidRequestQueueStage insertAutoVoidRequestQueueStage;
    private readonly LoggingService loggingService;
    private readonly MarkTicketsStage markTicketsStage;
    private readonly AddRequestDuplicatedTicketsServiceImp service;

    public AddRequestDuplicatedTicketsServiceTests()
    {
        getDuplicatedTicketsStage = Substitute.For<GetDuplicatedTicketsStage>();
        checkDuplicatedTicketsStage = Substitute.For<CheckDuplicatedTicketsStage>();
        markTicketsStage = Substitute.For<MarkTicketsStage>();
        addRequestStage = Substitute.For<AddRequestStage>();
        insertAutoVoidRequestQueueStage = Substitute.For<InsertAutoVoidRequestQueueStage>();
        loggingService = Substitute.For<LoggingService>();
        service = new AddRequestDuplicatedTicketsServiceImp(getDuplicatedTicketsStage, checkDuplicatedTicketsStage,
            markTicketsStage, addRequestStage, insertAutoVoidRequestQueueStage, loggingService);
    }

    [Fact]
    public async Task Execute_HasException_CalledExpected()
    {
        // Arrange
        getDuplicatedTicketsStage.Execute().Throws(new Exception());

        // Act
        await service.Execute();

        // Assert
        await loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<List<string>>());
        await loggingService.Received(1)
            .ErrorAsync($"{nameof(AddRequestDuplicatedTicketsService)} Error", Arg.Any<Exception>());
    }

    [Fact]
    public async Task Execute_NoHasTickets_Return()
    {
        // Arrange
        DuplicatedTickets? tickets = null;
        getDuplicatedTicketsStage.Execute().ReturnsForAnyArgs(tickets);

        // Act
        await service.Execute();

        // Assert
        await getDuplicatedTicketsStage.Received(1).Execute();
        await checkDuplicatedTicketsStage.DidNotReceiveWithAnyArgs().Execute(Arg.Any<string>());
    }

    [Fact]
    public async Task Execute_NoHasTransInfoJson_Return()
    {
        // Arrange
        var tickets = new DuplicatedTickets();
        getDuplicatedTicketsStage.Execute().ReturnsForAnyArgs(tickets);

        // Act
        await service.Execute();

        // Assert
        await getDuplicatedTicketsStage.Received(1).Execute();
        await checkDuplicatedTicketsStage.DidNotReceiveWithAnyArgs().Execute(tickets.TransInfoJson);
    }

    [Fact]
    public async Task Execute_NoValidations_Return()
    {
        // Arrange
        var tickets = new DuplicatedTickets { TransInfoJson = "123" };
        getDuplicatedTicketsStage.Execute().ReturnsForAnyArgs(tickets);
        var validations = new List<CheckDuplicatedTicketsResult>();
        checkDuplicatedTicketsStage.Execute(tickets.TransInfoJson).ReturnsForAnyArgs(validations);

        // Act
        await service.Execute();

        // Assert
        await getDuplicatedTicketsStage.Received(1).Execute();
        await checkDuplicatedTicketsStage.Received(1).Execute(tickets.TransInfoJson);
        await insertAutoVoidRequestQueueStage.DidNotReceiveWithAnyArgs()
            .Execute(Arg.Any<int>(), Arg.Any<CheckDuplicatedTicketsResult>(), Arg.Any<List<string>>());
    }

    [Fact]
    public async Task Execute_ValidData_CallExpected()
    {
        // Arrange
        var tickets = new DuplicatedTickets { TransInfoJson = "123" };
        var validation1 = new CheckDuplicatedTicketsResult { MarkTransIds = "123" };
        var validation3 = new CheckDuplicatedTicketsResult { MarkTransIds = "1" };
        var validations = new List<CheckDuplicatedTicketsResult>
            { validation1, new() { MarkTransIds = "456" }, validation3 };
        getDuplicatedTicketsStage.Execute().ReturnsForAnyArgs(tickets);
        checkDuplicatedTicketsStage.Execute(tickets.TransInfoJson).ReturnsForAnyArgs(validations);
        markTicketsStage.Execute("123", Arg.Any<List<string>>()).Returns(true);
        markTicketsStage.Execute("456", Arg.Any<List<string>>()).Returns(false);
        markTicketsStage.Execute("1", Arg.Any<List<string>>()).Returns(true);

        addRequestStage.Execute(validation1, Arg.Any<List<string>>())
            .Returns(5);
        addRequestStage.Execute(validation3, Arg.Any<List<string>>())
            .Returns(0);
        // Act
        await service.Execute();

        // Assert
        await insertAutoVoidRequestQueueStage.Received(1)
            .Execute(5, Arg.Any<CheckDuplicatedTicketsResult>(), Arg.Any<List<string>>());
        await loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<List<string>>());
    }
}