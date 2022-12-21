using NSubstitute;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest
{
    public class GetTicketsStageImpTests
    {
        [Fact]
        public async Task Execute_HasSingleTickets_CalledExpected()
        {
            const int siteDbGroupId = 0;
            var singleTickets = new List<RequestDetailTicketModel>
            {
                new()
            };
            var parlayTickets = new List<RequestDetailTicketModel>();
            var queue = new AutoVoidRequestQueueModel();
            var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
            var requestTicketService = Substitute.For<RequestTicketService>();
            requestTicketService.GetTicketsByRequestIdAndSiteDbGroupId(default, default)
                .ReturnsForAnyArgs((singleTickets, parlayTickets));
            var service = new GetTicketsStageImp(autoVoidRequestQueueService, requestTicketService);

            var actual = await service.Execute(siteDbGroupId, queue, new List<string>());

            await requestTicketService.Received(1)
                .GetTicketsByRequestIdAndSiteDbGroupId(Arg.Any<int>(), Arg.Any<int>());
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
            var queue = new AutoVoidRequestQueueModel();
            var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
            var requestTicketService = Substitute.For<RequestTicketService>();
            requestTicketService.GetTicketsByRequestIdAndSiteDbGroupId(default, default)
                .ReturnsForAnyArgs((singleTickets, parlayTickets));
            var service = new GetTicketsStageImp(autoVoidRequestQueueService, requestTicketService);

            var actual = await service.Execute(siteDbGroupId, queue, new List<string>());

            await requestTicketService.Received(1)
                .GetTicketsByRequestIdAndSiteDbGroupId(Arg.Any<int>(), Arg.Any<int>());
            await autoVoidRequestQueueService.Received(1)
                .UpdateQueue(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<string>());
            Assert.Null(actual.Item2);
            Assert.False(actual.Item1);
        }
    }
}