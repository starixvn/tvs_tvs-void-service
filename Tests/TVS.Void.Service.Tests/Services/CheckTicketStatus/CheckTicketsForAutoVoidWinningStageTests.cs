using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.CheckTicketStatus;

namespace TVS.Void.Service.Tests.Services.CheckTicketStatus;

public class CheckTicketsForAutoVoidWinningStageTests : IClassFixture<AppSettingFixture>
{
    private readonly InsertAutoVoidQueueStage insertAutoVoidQueueStage;
    private readonly RequestTicketService requestTicketService;
    private readonly CheckTicketsForAutoVoidWinningStageImp service;

    public CheckTicketsForAutoVoidWinningStageTests()
    {
        requestTicketService = Substitute.For<RequestTicketService>();
        insertAutoVoidQueueStage = Substitute.For<InsertAutoVoidQueueStage>();
        service = new CheckTicketsForAutoVoidWinningStageImp(requestTicketService, insertAutoVoidQueueStage);
    }

    [Fact]
    public async Task Execute_WhenTicketsEmpty_NotExecuteAnyThing()
    {
        // Arrange
        var groupTickets = new GroupTicketsInRequest
        {
            SingleTickets = [],
            ParlayTickets = [],
            CricketTickets = []
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
            SingleTickets = [new() { SiteDbGroupId = 1 }],
            ParlayTickets = [new() { SiteDbGroupId = 2 }],
            CricketTickets = [new() { SiteDbGroupId = 3 }]
        };
        var logData = new List<string>();
        var checkResult = new CheckStatusTicketInsertAutoVoidQueueOutput
        {
            ValidRequests = [],
            InvalidSingleTickets = [],
            InvalidParlayTickets = [],
            InvalidCricketTickets = []
        };
        requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(default,
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>()).ReturnsForAnyArgs(checkResult);

        // Act
        await service.Execute(logData, groupTickets);

        // Assert
        await requestTicketService.Received().CheckStatusTicketForInsertAutoVoidQueue(Arg.Any<int>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>());
        await insertAutoVoidQueueStage.Received().ExecuteForAutoVoidWinning(Arg.Any<int>(),
            checkResult.ValidRequests, Arg.Any<IDictionary<int, string>>(), Arg.Any<List<string>>());
        await requestTicketService.Received().UpdateIsProcessed(checkResult.InvalidSingleTickets,
            checkResult.InvalidParlayTickets, checkResult.InvalidCricketTickets);
    }

    [Fact]
    public async Task Execute_HasTickets_LogDataContainsExpectedMessages()
    {
        // Arrange
        var groupTickets = new GroupTicketsInRequest
        {
            SingleTickets = [new() { SiteDbGroupId = 1 }],
            ParlayTickets = [new() { SiteDbGroupId = 2 }],
            CricketTickets = [new() { SiteDbGroupId = 3 }]
        };
        var logData = new List<string>();
        var checkResult = new CheckStatusTicketInsertAutoVoidQueueOutput
        {
            ValidRequests = [],
            InvalidSingleTickets = [],
            InvalidParlayTickets = [],
            InvalidCricketTickets = []
        };
        requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(default,
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>()).ReturnsForAnyArgs(checkResult);

        // Act
        await service.Execute(logData, groupTickets);

        // Assert
        Assert.Contains(logData, x => x.Contains("CheckTicketsForAutoVoidWinningStage"));
        Assert.Contains(logData, x => x.Contains("SingleTickets.Count: 1"));
        Assert.Contains(logData, x => x.Contains("ParlayTickets.Count: 1"));
        Assert.Contains(logData, x => x.Contains("CricketTickets.Count: 1"));
        Assert.Contains(logData, x => x.Contains("broadcastSessionIdCacheByMatch"));
        Assert.Contains(logData, x => x.Contains("ServerTime"));
    }

    [Fact]
    public async Task Execute_HasTickets_ProcessesEachServerSeparately()
    {
        // Arrange
        var groupTickets = new GroupTicketsInRequest
        {
            SingleTickets = [new() { SiteDbGroupId = 1 }, new() { SiteDbGroupId = 2 }],
            ParlayTickets = [],
            CricketTickets = []
        };
        var logData = new List<string>();
        var checkResult = new CheckStatusTicketInsertAutoVoidQueueOutput
        {
            ValidRequests = [],
            InvalidSingleTickets = [],
            InvalidParlayTickets = [],
            InvalidCricketTickets = []
        };
        requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(default,
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>()).ReturnsForAnyArgs(checkResult);

        // Act
        await service.Execute(logData, groupTickets);

        // Assert
        // Should be called for each server in SiteDbGroupConstant.ServerList
        var expectedCallCount = SiteDbGroupConstant.ServerList.Count;
        await requestTicketService.Received(expectedCallCount).CheckStatusTicketForInsertAutoVoidQueue(Arg.Any<int>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>());
        await insertAutoVoidQueueStage.Received(expectedCallCount).ExecuteForAutoVoidWinning(Arg.Any<int>(),
            checkResult.ValidRequests, Arg.Any<IDictionary<int, string>>(), Arg.Any<List<string>>());
        await requestTicketService.Received(expectedCallCount).UpdateIsProcessed(checkResult.InvalidSingleTickets,
            checkResult.InvalidParlayTickets, checkResult.InvalidCricketTickets);
    }

    [Fact]
    public async Task Execute_HasTickets_FiltersTicketsBySiteDbGroupId()
    {
        // Arrange
        var groupTickets = new GroupTicketsInRequest
        {
            SingleTickets = [new RequestDetailTicketModel { SiteDbGroupId = 1 }, new RequestDetailTicketModel { SiteDbGroupId = 2 }],
            ParlayTickets = [new RequestDetailTicketModel { SiteDbGroupId = 1 }],
            CricketTickets = [new RequestDetailTicketModel { SiteDbGroupId = 2 }]
        };
        var logData = new List<string>();
        var checkResult = new CheckStatusTicketInsertAutoVoidQueueOutput
        {
            ValidRequests = [],
            InvalidSingleTickets = [],
            InvalidParlayTickets = [],
            InvalidCricketTickets = []
        };
        requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(default,
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>()).ReturnsForAnyArgs(checkResult);

        // Act
        await service.Execute(logData, groupTickets);

        // Assert
        // Verify that CheckStatusTicketForInsertAutoVoidQueue is called with filtered tickets for each server
        await requestTicketService.Received().CheckStatusTicketForInsertAutoVoidQueue(1,
            Arg.Is<IEnumerable<RequestDetailTicketModel>>(x => x.Any(t => t.SiteDbGroupId == 1)),
            Arg.Is<IEnumerable<RequestDetailTicketModel>>(x => x.Any(t => t.SiteDbGroupId == 1)),
            Arg.Is<IEnumerable<RequestDetailTicketModel>>(x => !x.Any()));
        await requestTicketService.Received().CheckStatusTicketForInsertAutoVoidQueue(2,
            Arg.Is<IEnumerable<RequestDetailTicketModel>>(x => x.Any(t => t.SiteDbGroupId == 2)),
            Arg.Is<IEnumerable<RequestDetailTicketModel>>(x => !x.Any()),
            Arg.Is<IEnumerable<RequestDetailTicketModel>>(x => x.Any(t => t.SiteDbGroupId == 2)));
    }

    [Fact]
    public async Task Execute_HasTickets_LogsCheckResultForEachServer()
    {
        // Arrange
        var groupTickets = new GroupTicketsInRequest
        {
            SingleTickets = [new() { SiteDbGroupId = 1 }],
            ParlayTickets = [],
            CricketTickets = []
        };
        var logData = new List<string>();
        var checkResult = new CheckStatusTicketInsertAutoVoidQueueOutput
        {
            ValidRequests = [],
            InvalidSingleTickets = [],
            InvalidParlayTickets = [],
            InvalidCricketTickets = []
        };
        requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(default,
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>()).ReturnsForAnyArgs(checkResult);

        // Act
        await service.Execute(logData, groupTickets);

        // Assert
        // Should contain log entries for each server
        foreach (var server in SiteDbGroupConstant.ServerList)
        {
            Assert.Contains(logData, x => x.Contains($"[{server.Id}]CheckStatusTicketForInsertAutoVoidQueue"));
        }
    }

    [Fact]
    public async Task Execute_HasTickets_UpdatesBroadcastSessionIdCache()
    {
        // Arrange
        var groupTickets = new GroupTicketsInRequest
        {
            SingleTickets = [new() { SiteDbGroupId = 1 }],
            ParlayTickets = [],
            CricketTickets = []
        };
        var logData = new List<string>();
        var checkResult = new CheckStatusTicketInsertAutoVoidQueueOutput
        {
            ValidRequests = [],
            InvalidSingleTickets = [],
            InvalidParlayTickets = [],
            InvalidCricketTickets = []
        };
        requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(default,
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>()).ReturnsForAnyArgs(checkResult);

        // Act
        await service.Execute(logData, groupTickets);

        // Assert
        // Verify that ExecuteForAutoVoidWinning is called with the same dictionary instance
        await insertAutoVoidQueueStage.Received().ExecuteForAutoVoidWinning(Arg.Any<int>(),
            checkResult.ValidRequests, Arg.Is<IDictionary<int, string>>(x => x != null), logData);
    }
}