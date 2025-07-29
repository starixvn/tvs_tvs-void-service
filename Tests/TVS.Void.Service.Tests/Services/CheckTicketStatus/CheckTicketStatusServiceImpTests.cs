using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.CheckTicketStatus;

namespace TVS.Void.Service.Tests.Services.CheckTicketStatus;

public class CheckTicketStatusServiceImpTests
{
    private readonly CheckTicketsForAutoVoidWinningStage checkTicketsForAutoVoidWinningStage;
    private readonly CheckTicketsForAutoVoidAllResultKeyedStage checkTicketsForAutoVoidAllResultKeyedStage;
    private readonly LoggingService loggingService;
    private readonly RequestTicketService requestTicketService;
    private readonly CheckTicketStatusServiceImp service;

    public CheckTicketStatusServiceImpTests()
    {
        requestTicketService = Substitute.For<RequestTicketService>();
        checkTicketsForAutoVoidWinningStage = Substitute.For<CheckTicketsForAutoVoidWinningStage>();
        checkTicketsForAutoVoidAllResultKeyedStage = Substitute.For<CheckTicketsForAutoVoidAllResultKeyedStage>();
        loggingService = Substitute.For<LoggingService>();
        service = new CheckTicketStatusServiceImp(requestTicketService, checkTicketsForAutoVoidWinningStage, checkTicketsForAutoVoidAllResultKeyedStage, loggingService);
    }

    [Fact]
    public async Task Execute_ExecuteNormalFlow_CallExpected()
    {
        // Arrange
        var ticketAutoVoidWinning = new RequestDetailTicketModel
        { VoidActionId = VoidActionTypeConstant.AutoVoidWinningId };
        var ticketAutoVoidAllResultKeyed = new RequestDetailTicketModel
        { VoidActionId = VoidActionTypeConstant.AutoVoidAllResultKeyedId };
        var getTicketsResult = new GroupTicketsInRequest
        {
            SingleTickets = [ticketAutoVoidWinning, ticketAutoVoidAllResultKeyed],
            ParlayTickets = [ticketAutoVoidWinning, ticketAutoVoidAllResultKeyed],
            CricketTickets = [ticketAutoVoidWinning, ticketAutoVoidAllResultKeyed]
        };
        requestTicketService.GetTicketInAllRequests().ReturnsForAnyArgs(getTicketsResult);

        // Act
        await service.Execute();

        // Assert
        await checkTicketsForAutoVoidWinningStage.Received(1).Execute(Arg.Any<List<string>>(), Arg.Any<GroupTicketsInRequest>());
        await checkTicketsForAutoVoidAllResultKeyedStage.Received(1).Execute(Arg.Any<List<string>>(), Arg.Any<GroupTicketsInRequest>());
        await loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<IList<string>>());
    }

    [Fact]
    public async Task Execute_HandleException_CallExpected()
    {
        // Arrange
        requestTicketService.GetTicketInAllRequests().Throws(new Exception());

        // Act
        await service.Execute();

        // Assert
        await loggingService.Received(1).InfoAppendNewLineAsync(Arg.Any<List<string>>());
        await loggingService.Received(1).ErrorAsync(Arg.Any<string>(), Arg.Any<Exception>());
    }
}