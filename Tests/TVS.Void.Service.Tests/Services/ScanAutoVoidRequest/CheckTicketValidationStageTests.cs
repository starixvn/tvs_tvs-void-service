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
        var results = new List<RequestDetailTicketModel>
        {
            new() { ValidationType = 1 },
            new() { ValidationType = 2 },
            new() { ValidationType = 3 },
            new() { ValidationType = 4 },
            new() { ValidationType = 5 },
            new() { ValidationType = 6 },
            new() { ValidationType = 7 },
            new() { ValidationType = 8 },
            new() { ValidationType = 9 }
        };
        var expected = new CheckTicketValidationOutput
        {
            VoidSingleTickets = new List<RequestDetailTicketModel> { new() { ValidationType = 1 } },
            SingleTicketsResettle = new List<RequestDetailTicketModel> { new() { ValidationType = 2 } },
            VoidParlayTickets = new List<RequestDetailTicketModel> { new() { ValidationType = 3 } },
            ParlayTicketsResettle = new List<RequestDetailTicketModel> { new() { ValidationType = 4 } },
            VoidCricketTickets = new List<RequestDetailTicketModel> { new() { ValidationType = 5 } },
            CricketTicketsResettle = new List<RequestDetailTicketModel> { new() { ValidationType = 6 } },
            VoidSingleTickets14 = new List<RequestDetailTicketModel> { new() { ValidationType = 8 } },
            VoidParlayTickets14 = new List<RequestDetailTicketModel> { new() { ValidationType = 9 } },
            CancelCOTickets = new List<RequestDetailTicketModel> { new() { ValidationType = 7 } }
        };

        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchAsync<RequestDetailTicketModel>(Arg.Any<CheckTicketsAllowToVoidCriteria>())
            .ReturnsForAnyArgs(results);
        var service = new CheckTicketValidationStageImp(repository);

        var actual = await service.ExecuteForVoid(siteDbGroupId, default, default, default, default, new List<string>());

        await repository.Received(1).FetchAsync<RequestDetailTicketModel>(Arg.Any<CheckTicketsAllowToVoidCriteria>());
        AssertExtension.Equal(expected, actual);
    }

    [Fact]
    public async Task ExecuteForRecover_ReturnExpected()
    {
        const int siteDbGroupId = 0;
        var requestDetailTickets = new List<RequestDetailTicketModel>();
        var expected = new CheckTicketValidationOutput
        {
            RecoverSingleTickets = requestDetailTickets,
            RecoverParlayTickets = requestDetailTickets,
            RecoverCricketTickets = requestDetailTickets,
            RecoverSingleTickets14 = requestDetailTickets,
            RecoverParlayTickets14 = requestDetailTickets
        };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>(Arg.Any<CheckTicketsAllowToRecoverCriteria>())
            .ReturnsForAnyArgs((requestDetailTickets, requestDetailTickets, requestDetailTickets, requestDetailTickets, requestDetailTickets));
        var service = new CheckTicketValidationStageImp(repository);

        var actual = await service.ExecuteForRecover(siteDbGroupId, requestDetailTickets, requestDetailTickets, requestDetailTickets, new List<string>());

        await repository.Received(1).FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>(Arg.Any<CheckTicketsAllowToRecoverCriteria>());
        AssertExtension.Equal(expected, actual);
    }
}