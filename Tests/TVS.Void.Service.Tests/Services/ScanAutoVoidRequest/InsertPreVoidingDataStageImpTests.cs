using NSubstitute;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class InsertPreVoidingDataStageImpTests
{
    [Fact]
    public async Task Execute_HasSingleTickets_CalledExpected()
    {
        var expected = string.Empty;
        const int siteDbGroupId = 0;
        var singleTickets = new List<RequestDetailTicketModel> { new() };
        var parlayTickets = new List<RequestDetailTicketModel>();
        var cricketTickets = new List<RequestDetailTicketModel>();
        var singleTickets14 = new List<RequestDetailTicketModel>();
        var queue = new AutoVoidRequestQueueModel();
        var requestDetailService = Substitute.For<RequestDetailService>();
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var requestTicketService = Substitute.For<RequestTicketService>();
        var notificationService = Substitute.For<NotificationService>();
        requestDetailService.InsertVoidingHistory(Arg.Any<string>(), default, Arg.Any<string>(),
                Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
                Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>())
            .ReturnsForAnyArgs(new List<VoidingHistoryModel>());

        var service = new InsertPreVoidingDataStageImp(requestDetailService, ticketVoidingQueueService,
            requestTicketService, notificationService);

        var actual = await service.Execute(siteDbGroupId, queue, singleTickets, parlayTickets, cricketTickets,
            singleTickets14, new List<string>());

        await requestDetailService.Received().InsertVoidingHistory(Arg.Any<string>(), default, Arg.Any<string>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>(),
            Arg.Any<IEnumerable<RequestDetailTicketModel>>(), Arg.Any<IEnumerable<RequestDetailTicketModel>>());
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Execute_HasNotSingleOrParlayTickets_ReturnMessage()
    {
        const string expected = "No ticket found.";
        var singleTickets = new List<RequestDetailTicketModel>();
        var parlayTickets = new List<RequestDetailTicketModel>();
        var cricketTickets = new List<RequestDetailTicketModel>();
        var singleTickets14 = new List<RequestDetailTicketModel>();

        var service = new InsertPreVoidingDataStageImp(default, default, default, default);

        var actual = await service.Execute(default, default, singleTickets, parlayTickets, cricketTickets,
            singleTickets14, default);

        Assert.Equal(expected, actual);
    }
}