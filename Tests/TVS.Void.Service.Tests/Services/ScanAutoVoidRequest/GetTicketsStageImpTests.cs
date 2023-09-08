using NSubstitute;
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
        var singleTickets = new List<RequestDetailTicketModel>
        {
            new()
        };
        var parlayTickets = new List<RequestDetailTicketModel>();
        var cricketTickets = new List<RequestDetailTicketModel>();
        var getTicketsResult = new GroupTicketsInRequest
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets
        };
        var queue = new AutoVoidRequestQueueModel();
        requestTicketService.GetTicketsByRequestsAndSiteDbGroupId(default, default)
            .ReturnsForAnyArgs(getTicketsResult);

        var actual = await service.Execute(siteDbGroupId, queue, new List<string>());

        await requestTicketService.Received(1)
            .GetTicketsByRequestsAndSiteDbGroupId(Arg.Any<int>(), Arg.Any<string>());
        AssertExtension.Equal(singleTickets, actual.Item2.SingleTickets);
        AssertExtension.Equal(parlayTickets, actual.Item2.ParlayTickets);
        Assert.True(actual.Item1);
    }

    [Fact]
    public async Task Execute_HasNotSingleOrParlayTickets_CalledExpected()
    {
        const int siteDbGroupId = 0;
        var singleTickets = new List<RequestDetailTicketModel>();
        var parlayTickets = new List<RequestDetailTicketModel>();
        var cricketTickets = new List<RequestDetailTicketModel>();
        var queue = new AutoVoidRequestQueueModel();
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