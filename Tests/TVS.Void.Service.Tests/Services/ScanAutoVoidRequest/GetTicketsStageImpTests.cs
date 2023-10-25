using NSubstitute;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class GetTicketsStageImpTests
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly RequestTicketService requestTicketService;
    private readonly GetTicketsStageImp service;

    public GetTicketsStageImpTests()
    {
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        requestTicketService = Substitute.For<RequestTicketService>();
        service = new GetTicketsStageImp(autoVoidRequestQueueService, requestTicketService);
    }

    [Fact]
    public async Task Execute_HasSingleTickets_CalledExpected()
    {
        const int siteDbGroupId = 0;
        var singleTickets = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 }, new() { MatchId = 2, RequestId = 1 } };
        var parlayTickets = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 }, new() { MatchId = 2, RequestId = 1 } };
        var cricketTickets = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 }, new() { MatchId = 2, RequestId = 1 } };
        var getTicketsResult = new GroupTicketsInRequest
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets
        };
        var singleTicketsExpected = new List<RequestDetailTicketModel> { new() { MatchId = 1, RequestId = 1 }};
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

    [Fact]
    public async Task Execute_HasNotSingleOrParlayTickets_CalledExpected()
    {
        const int siteDbGroupId = 0;
        var singleTickets = new List<RequestDetailTicketModel>();
        var parlayTickets = new List<RequestDetailTicketModel>();
        var cricketTickets = new List<RequestDetailTicketModel>();
        var queue = new AutoVoidRequestQueueModel{Action = AutoVoidConstant.QueueActionVoidInstant, MatchId = 0, RequestIds = "1"};
        var getTicketsResult = new GroupTicketsInRequest
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets
        };
        requestTicketService.GetTicketsByRequestsAndSiteDbGroupId(default, default)
            .ReturnsForAnyArgs(getTicketsResult);

        var actual = await service.Execute(siteDbGroupId, queue, new List<string>());

        await requestTicketService.Received(1)
            .GetTicketsByRequestsAndSiteDbGroupId(Arg.Any<int>(), Arg.Any<string>());
        await autoVoidRequestQueueService.Received(1)
            .UpdateQueue(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<string>());
        Assert.Null(actual.Item2);
        Assert.False(actual.Item1);
    }
}