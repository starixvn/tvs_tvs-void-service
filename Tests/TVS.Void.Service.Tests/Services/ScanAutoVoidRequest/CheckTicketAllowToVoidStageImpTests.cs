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
        var tickets = new List<RequestDetailTicketModel>();
        IEnumerable<object>[] fakeData = { tickets, tickets, tickets, tickets, tickets, tickets };
        var expected = new CheckStatusTicketOutput
        {
            ValidVoidSingleTickets = tickets,
            SingleTicketsResettle = tickets,
            ValidVoidParlayTickets = tickets,
            ParlayTicketsResettle = tickets,
            ValidVoidCricketTickets = tickets,
            CricketTicketsResettle = tickets
        };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchMultipleAsync(default, default)
            .ReturnsForAnyArgs(fakeData);
        var service = new CheckTicketAllowToVoidStageImp(repository);

        var actual = await service.Execute(siteDbGroupId, tickets, tickets, tickets, new List<string>());

        await repository.Received(1)
            .FetchMultipleAsync(Arg.Any<CheckTicketsAllowToVoidCriteria>(), Arg.Any<Type[]>());
        AssertExtension.Equal(expected, actual);
    }
}