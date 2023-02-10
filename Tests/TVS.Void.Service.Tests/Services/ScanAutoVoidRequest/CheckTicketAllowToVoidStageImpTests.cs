using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class CheckTicketAllowToVoidStageImpTests
{
    [Fact]
    public async Task Execute_ReturnExpected()
    {
        const int siteDbGroupId = 0;
        var singleTickets = new List<RequestDetailTicketModel>();
        var parlayTickets = new List<RequestDetailTicketModel>();
        IEnumerable<object>[] fakeData = { singleTickets, singleTickets, singleTickets, singleTickets };
        var expected = new CheckStatusTicketOutput
        {
            ValidVoidSingleTickets = singleTickets,
            ValidVoidParlayTickets = singleTickets,
            SingleTicketsResettle = singleTickets,
            ParlayTicketsResettle = singleTickets
        };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchMultipleAsync(default, default)
            .ReturnsForAnyArgs(fakeData);
        var service = new CheckTicketAllowToVoidStageImp(repository);

        var actual = await service.Execute(siteDbGroupId, singleTickets, parlayTickets, new List<string>());

        await repository.Received(1)
            .FetchMultipleAsync(Arg.Any<CheckTicketsAllowToVoidCriteria>(), Arg.Any<Type[]>());
        AssertExtension.Equal(expected, actual);
    }
}