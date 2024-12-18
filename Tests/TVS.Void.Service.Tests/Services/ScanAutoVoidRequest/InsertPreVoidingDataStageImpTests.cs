using NSubstitute;
using TVS.Void.Service.Constants;
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
        var queue = new AutoVoidRequestQueueModel();
        const string voidDataXml = "xmlAbc";
        var requestDetailService = Substitute.For<RequestDetailService>();
        var ticketVoidingQueueService = Substitute.For<TicketVoidingQueueService>();
        var requestTicketService = Substitute.For<RequestTicketService>();
        var notificationService = Substitute.For<NotificationService>();
        requestDetailService.InsertVoidingHistory(Arg.Any<string>(), default, Arg.Any<string>(), voidDataXml)
            .ReturnsForAnyArgs(new List<VoidingHistoryModel>());

        var service = new InsertPreVoidingDataStageImp(requestDetailService, ticketVoidingQueueService,
            requestTicketService, notificationService);

        var actual = await service.Execute(siteDbGroupId, queue, singleTickets, voidDataXml, new List<string>());

        await requestDetailService.Received().InsertVoidingHistory(Arg.Any<string>(), default, Arg.Any<string>(), voidDataXml);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Execute_HasNotSingleOrParlayTickets_ReturnMessage()
    {
        const string expected = "No ticket found.";
        var singleTickets = new List<RequestDetailTicketModel>();

        var service = new InsertPreVoidingDataStageImp(default, default, default, default);

        var actual = await service.Execute(default, default, singleTickets, default, default);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MapVoidingTypeToTickets_ReturnExpected()
    {
        var validation = new CheckTicketValidationOutput
        {
            VoidSingleTickets = null,
            VoidParlayTickets = new List<RequestDetailTicketModel>(),
            VoidCricketTickets = new List<RequestDetailTicketModel> { new() { TransId = 1 } },
            VoidSingleTickets14 = new List<RequestDetailTicketModel>(),
            VoidParlayTickets14 = new List<RequestDetailTicketModel>(),
            RecoverSingleTickets = new List<RequestDetailTicketModel>(),
            RecoverParlayTickets = new List<RequestDetailTicketModel>(),
            RecoverCricketTickets = new List<RequestDetailTicketModel>(),
            RecoverSingleTickets14 = new List<RequestDetailTicketModel>(),
            RecoverParlayTickets14 = new List<RequestDetailTicketModel>()
        };
        var groupTickets = new List<RequestDetailTicketModel> { new() { TransId = 1, VoidingTypeId = VoidingTypeConstant.Cricket } };
        var service = new InsertPreVoidingDataStageImp(default, default, default, default);

        var actual = service.MapVoidingTypeToTickets(validation);

        AssertExtension.Equal(groupTickets, actual);
    }
}