using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.CheckTicketStatus;

namespace TVS.Void.Service.Tests.Services.CheckTicketStatus;

public class CheckTicketStatusServiceImpTests
{
    [Fact]
    public async Task Execute_WhenTicketsEmpty_ShouldNotWriteLog()
    {
        // Arrange
        var fakeRequestsList = new List<RequestModel> { new() { RequestId = 112 } };
        var requestTicketService = Substitute.For<RequestTicketService>();
        var insertAutoVoidQueueStage = Substitute.For<InsertAutoVoidQueueStage>();
        var loggingService = Substitute.For<LoggingService>();
        requestTicketService.GetTicketInAllRequests().ReturnsForAnyArgs((new List<RequestDetailTicketModel>(), new List<RequestDetailTicketModel>()));
        requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(default, default, default).ReturnsForAnyArgs(fakeRequestsList);
        var service = new CheckTicketStatusServiceImp(requestTicketService, insertAutoVoidQueueStage, loggingService);

        // Act
        await service.Execute();

        // Assert
        await requestTicketService.Received(1).GetTicketInAllRequests();
        await requestTicketService.DidNotReceiveWithAnyArgs().CheckStatusTicketForInsertAutoVoidQueue(Arg.Any<int>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>());
        await insertAutoVoidQueueStage.DidNotReceiveWithAnyArgs()
            .Execute(Arg.Any<int>(), Arg.Any<IEnumerable<RequestModel>>(),
                Arg.Any<string>(), Arg.Any<List<string>>());
        await loggingService.DidNotReceiveWithAnyArgs().InfoAppendNewLineAsync(default);
    }

    [Theory]
    [MemberData(nameof(ExecuteWhenTicketsNotEmptyTestData))]
    public async Task Execute_WhenTicketsNotEmpty_ShouldWriteLog(IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
    {
        // Arrange
        var totalSiteDbCount = SiteDbGroupConstant.ServerList.Count;
        var fakeRequestsList = new List<RequestModel> { new() { RequestId = 112 } };
        var requestTicketService = Substitute.For<RequestTicketService>();
        var insertAutoVoidQueueStage = Substitute.For<InsertAutoVoidQueueStage>();
        var loggingService = Substitute.For<LoggingService>();
        requestTicketService.GetTicketInAllRequests().ReturnsForAnyArgs((singleTickets, parlayTickets));
        requestTicketService.CheckStatusTicketForInsertAutoVoidQueue(default, default, default).ReturnsForAnyArgs(fakeRequestsList);
        var service = new CheckTicketStatusServiceImp(requestTicketService, insertAutoVoidQueueStage, loggingService);

        // Act
        await service.Execute();

        // Assert
        await requestTicketService.Received(1).GetTicketInAllRequests();
        await requestTicketService.Received(totalSiteDbCount).CheckStatusTicketForInsertAutoVoidQueue(Arg.Any<int>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>());
        await insertAutoVoidQueueStage.Received(totalSiteDbCount)
            .Execute(Arg.Any<int>(), Arg.Is<IEnumerable<RequestModel>>(x => x == fakeRequestsList),
                Arg.Any<string>(), Arg.Any<List<string>>());
        await loggingService.ReceivedWithAnyArgs(1).InfoAppendNewLineAsync(default);
    }

    [Fact]
    public async Task Execute_ShouldExecuteCatchException()
    {
        // Arrange
        var requestTicketService = Substitute.For<RequestTicketService>();
        var insertAutoVoidQueueStage = Substitute.For<InsertAutoVoidQueueStage>();
        var loggingService = Substitute.For<LoggingService>();
        requestTicketService.WhenForAnyArgs(x => x.GetTicketInAllRequests())
            .Do(x => throw new Exception());
        var service = new CheckTicketStatusServiceImp(requestTicketService, insertAutoVoidQueueStage, loggingService);

        // Act
        await service.Execute();

        // Assert
        await requestTicketService.Received(1).GetTicketInAllRequests();
        await insertAutoVoidQueueStage.DidNotReceiveWithAnyArgs().Execute(default, default, default, default);
        await loggingService.ReceivedWithAnyArgs(1).InfoAppendNewLineAsync(default);
        await loggingService.ReceivedWithAnyArgs(1).ErrorAsync(default, default);
    }

    public static IEnumerable<object[]> ExecuteWhenTicketsNotEmptyTestData =>
        new List<object[]>
        {
            new object[] {   new List<RequestDetailTicketModel> {new() { SiteDbGroupId = 0, TransId = 113 }}, new List<RequestDetailTicketModel>() },
            new object[] { new List<RequestDetailTicketModel>(), new List<RequestDetailTicketModel>{new() { SiteDbGroupId = 1, TransId = 222 } }},
            new object[] {  new List<RequestDetailTicketModel> {new() { SiteDbGroupId = 0, TransId = 113 }}, new List<RequestDetailTicketModel>{new() { SiteDbGroupId = 1, TransId = 222 } }},
        };
}