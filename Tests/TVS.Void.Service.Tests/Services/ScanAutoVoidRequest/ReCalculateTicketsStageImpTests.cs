using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class ReCalculateTicketsStageImpTests
{
    [Fact]
    public async Task Execute_HasSingleTickets_CalledExpected()
    {
        const int siteDbGroupId = 0;
        var singleTickets = new List<RequestDetailTicketModel>
        {
            new()
        };
        var requestTicketService = Substitute.For<RequestTicketService>();
        var repository = Substitute.For<IDynamicRepository>();

        var service = new ReCalculateTicketsStageImp(repository, requestTicketService);

        await service.Execute(siteDbGroupId, singleTickets, [], new List<RequestDetailTicketModel>(), new List<string>());

        await repository.Received(1)
            .FetchAsync<UnMarkReCalculateResult>(Arg.Is<UnMarkAndReCalculateTicketsCriteria>(x => x.SiteDbGroupId == siteDbGroupId));
    }

    [Fact]
    public async Task Execute_HasParlayTickets_CalledExpected()
    {
        const int siteDbGroupId = 0;
        var parlayTickets = new List<RequestDetailTicketModel>
        {
            new()
        };
        var requestTicketService = Substitute.For<RequestTicketService>();
        var repository = Substitute.For<IDynamicRepository>();

        var service = new ReCalculateTicketsStageImp(repository, requestTicketService);

        await service.Execute(siteDbGroupId, [], parlayTickets, new List<RequestDetailTicketModel>(), new List<string>());

        await repository.Received(1)
           .FetchAsync<UnMarkReCalculateResult>(Arg.Is<UnMarkAndReCalculateTicketsCriteria>(x => x.SiteDbGroupId == siteDbGroupId));
    }
}