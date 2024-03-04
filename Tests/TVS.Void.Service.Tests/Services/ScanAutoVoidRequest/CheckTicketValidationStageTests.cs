using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class CheckTicketValidationStageTests
{
    [Fact]
    public async Task ExecuteForVoid_ReturnExpected()
    {
        const int siteDbGroupId = 0;
        var requestDetailTickets = new List<RequestDetailTicketModel>();
        IEnumerable<object>[] fakeData = { requestDetailTickets, requestDetailTickets, requestDetailTickets, requestDetailTickets, requestDetailTickets,
            requestDetailTickets, requestDetailTickets, requestDetailTickets, requestDetailTickets };
        var expected = new CheckTicketValidationOutput
        {
            VoidSingleTickets = requestDetailTickets,
            SingleTicketsResettle = requestDetailTickets,
            VoidParlayTickets = requestDetailTickets,
            ParlayTicketsResettle = requestDetailTickets,
            VoidCricketTickets = requestDetailTickets,
            CricketTicketsResettle = requestDetailTickets,
            VoidSingleTickets14 = requestDetailTickets,
            VoidParlayTickets14 = requestDetailTickets
        };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchMultipleAsync(default, default)
            .ReturnsForAnyArgs(fakeData);
        var service = new CheckTicketValidationStageImp(repository);

        var actual = await service.ExecuteForVoid(siteDbGroupId, requestDetailTickets, requestDetailTickets, requestDetailTickets, new List<string>());

        await repository.Received(1).FetchMultipleAsync(Arg.Any<CheckTicketsAllowToVoidCriteria>(), Arg.Any<Type[]>());
        AssertExtension.Equal(expected, actual);
    }

    [Fact]
    public async Task ExecuteForRecover_ReturnExpected()
    {
        const int siteDbGroupId = 0;
        var requestDetailTickets = new List<RequestDetailTicketModel>();
        IEnumerable<object>[] fakeData = { requestDetailTickets, requestDetailTickets, requestDetailTickets, requestDetailTickets, requestDetailTickets };
        var expected = new CheckTicketValidationOutput
        {
            RecoverSingleTickets = requestDetailTickets,
            RecoverParlayTickets = requestDetailTickets,
            RecoverCricketTickets = requestDetailTickets,
            RecoverSingleTickets14 = requestDetailTickets,
            RecoverParlayTickets14 = requestDetailTickets
        };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchMultipleAsync(default, default)
            .ReturnsForAnyArgs(fakeData);
        var service = new CheckTicketValidationStageImp(repository);

        var actual = await service.ExecuteForRecover(siteDbGroupId, requestDetailTickets, requestDetailTickets, requestDetailTickets, new List<string>());

        await repository.Received(1).FetchMultipleAsync(Arg.Any<CheckTicketsAllowToRecoverCriteria>(), Arg.Any<Type[]>());
        AssertExtension.Equal(expected, actual);
    }
}