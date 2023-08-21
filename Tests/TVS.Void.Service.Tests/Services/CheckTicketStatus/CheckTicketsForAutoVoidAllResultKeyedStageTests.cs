using NSubstitute;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.CheckTicketStatus;

namespace TVS.Void.Service.Tests.Services.CheckTicketStatus;

public class CheckTicketsForAutoVoidAllResultKeyedStageTests : IClassFixture<AppSettingFixture>
{
    private readonly InsertAutoVoidQueueStage insertAutoVoidQueueStage;
    private readonly RequestTicketService requestTicketService;
    private readonly CheckTicketsForAutoVoidAllResultKeyedStageImp service;

    public CheckTicketsForAutoVoidAllResultKeyedStageTests()
    {
        requestTicketService = Substitute.For<RequestTicketService>();
        insertAutoVoidQueueStage = Substitute.For<InsertAutoVoidQueueStage>();
        service = new CheckTicketsForAutoVoidAllResultKeyedStageImp(requestTicketService, insertAutoVoidQueueStage);
    }

    [Fact]
    public async Task Execute_WhenTicketsEmpty_NotExecuteAnyThing()
    {
        // Arrange
        var groupTickets = new GroupTicketsInRequest
        {
            SingleTickets = new List<RequestDetailTicketModel>(),
            ParlayTickets = new List<RequestDetailTicketModel>(),
            CricketTickets = new List<RequestDetailTicketModel>()
        };
        var logData = new List<string>();

        // Act
        await service.Execute(logData, groupTickets);

        // Assert
        await requestTicketService.DidNotReceiveWithAnyArgs().CheckStatusTicketForInsertAutoVoidQueue(default,
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>());
    }

    [Fact]
    public async Task Execute_HasTickets_CalledExpected()
    {
        // Arrange
        var groupTickets = new GroupTicketsInRequest
        {
            SingleTickets = new List<RequestDetailTicketModel> { new() { SiteDbGroupId = 0 } },
            ParlayTickets = new List<RequestDetailTicketModel> { new() { SiteDbGroupId = 0 } },
            CricketTickets = new List<RequestDetailTicketModel> { new() { SiteDbGroupId = 0 } }
        };
        var logData = new List<string>();
        var checkResult = new CheckStatusTicketInsertAutoVoidQueueOutput
        {
            ValidRequests = new List<RequestModel>(),
            InvalidParlayTickets = new List<RequestDetailTicketModel>()
        };
        requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(default,
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>()).ReturnsForAnyArgs(checkResult);

        // Act
        await service.Execute(logData, groupTickets);

        // Assert
        await requestTicketService.Received(4).CheckStatusTicketForInsertAutoVoidQueue(Arg.Any<int>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>());
        await insertAutoVoidQueueStage.Received(4).ExecuteForAutoVoidAllResultKeyed(Arg.Any<int>(),
            checkResult.ValidRequests,
            Arg.Any<Dictionary<int, string>>(), Arg.Any<List<string>>());
    }
}