using NSubstitute;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class GetTicketsStageImpTests
{
    private readonly RequestTicketService requestTicketService;
    private readonly GetTicketsStageImp service;

    public GetTicketsStageImpTests()
    {
        requestTicketService = Substitute.For<RequestTicketService>();
        service = new GetTicketsStageImp(requestTicketService);
    }

    [Fact]
    public async Task Execute_HasSingleTickets_CalledExpected()
    {
        const int siteDbGroupId = 0;
        var singleTickets = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 }, new() { MatchId = 2, RequestId = 1 } };
        var parlayTickets = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 }, new() { MatchId = 2, RequestId = 1 } };
        var cricketTickets = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 }, new() { MatchId = 2, RequestId = 1 } };
        var cancelCoTickets = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 }, new() { MatchId = 2, RequestId = 1 } };

        var getTicketsResult = new GroupTicketsInRequest
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets,
            CancelCOTickets = cancelCoTickets
        };
        var singleTicketsExpected = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 } };
        var parlayTicketsExpected = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 } };
        var cricketTicketsExpected = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 } };
        var queue = new AutoVoidRequestQueueModel { Action = AutoVoidConstant.QueueActionVoidInstant, MatchId = 1, RequestIds = "1" };
        requestTicketService.GetTicketsByRequestsAndSiteDbGroupId(default, default)
            .ReturnsForAnyArgs(getTicketsResult);

        var actual = await service.Execute(siteDbGroupId, queue, new List<string>());

        await requestTicketService.Received(1)
            .GetTicketsByRequestsAndSiteDbGroupId(Arg.Any<int>(), Arg.Any<string>());
        AssertExtension.Equal(singleTicketsExpected, actual.Item2.SingleTickets);
        AssertExtension.Equal(parlayTicketsExpected, actual.Item2.ParlayTickets);
        AssertExtension.Equal(cricketTicketsExpected, actual.Item2.CricketTickets);
        Assert.True(actual.Item1);
    }

    [Theory]
    [InlineData(AutoVoidConstant.QueueActionVoidInstant, 0)]
    [InlineData(AutoVoidConstant.QueueActionVoidInstant, 1)]
    [InlineData(AutoVoidConstant.QueueActionVoidResultKeyed, 0)]
    public async Task Execute_HasNotSingleOrParlayTickets_CalledExpected(string action, int matchId)
    {
        const int siteDbGroupId = 0;
        var queue = new AutoVoidRequestQueueModel { Action = action, MatchId = matchId, RequestIds = "1" };
        var getTicketsResult = new GroupTicketsInRequest
        {
            SingleTickets = new List<RequestDetailTicketModel>(),
            ParlayTickets = new List<RequestDetailTicketModel>(),
            CricketTickets = new List<RequestDetailTicketModel>(),
            CancelCOTickets = new List<RequestDetailTicketModel>()
        };
        requestTicketService.GetTicketsByRequestsAndSiteDbGroupId(default, default)
            .ReturnsForAnyArgs(getTicketsResult);

        var actual = await service.Execute(siteDbGroupId, queue, new List<string>());

        await requestTicketService.Received(1).GetTicketsByRequestsAndSiteDbGroupId(siteDbGroupId, queue.RequestIds);
        Assert.Null(actual.Item2);
        Assert.False(actual.Item1);
    }
}