using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Tests.Services;

public class RequestTicketServiceImpTests
{
    [Theory]
    [InlineData(1, "2,3")]
    [InlineData(0, "1")]
    public async Task GetTicketsByRequestsAndSiteDbGroupId_ShouldReturnExpectedData(int siteDbGroupId, string requestIds)
    {
        // Arrange
        var singleTickets = new List<RequestDetailTicketModel> { new() { TransId = 113 } };
        var parlayTickets = new List<RequestDetailTicketModel> { new() { TransId = 222 } };
        var cricketTickets = new List<RequestDetailTicketModel> { new() { TransId = 222 } };
        var repository = Substitute.For<IDynamicRepository>();
        repository
            .FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>(default)
            .ReturnsForAnyArgs((singleTickets, parlayTickets, cricketTickets));
        var service = new RequestTicketServiceImp(repository);
        var expected = new GroupTicketsInRequest
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets
        };
        // Act
        var actual = await service.GetTicketsByRequestsAndSiteDbGroupId(siteDbGroupId, requestIds);

        // Assert
        await repository.Received(1).FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>
            (Arg.Is<GetRequestTicketsCriteria>(x => x.SiteDbGroupId == siteDbGroupId && x.RequestIds == requestIds));
        AssertExtension.Equal(expected, actual);
    }

    [Fact]
    public async Task GetTicketInAllRequests_ShouldReturnExpectedData()
    {
        // Arrange
        var singleTickets = new List<RequestDetailTicketModel> { new() { TransId = 113 } };
        var parlayTickets = new List<RequestDetailTicketModel> { new() { TransId = 222 } };
        var cricketTickets = new List<RequestDetailTicketModel> { new() { TransId = 222 } };
        var repository = Substitute.For<IDynamicRepository>();
        var expected = new GroupTicketsInRequest
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets
        };
        repository
            .FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>(default)
            .ReturnsForAnyArgs((singleTickets, parlayTickets, cricketTickets));
        var service = new RequestTicketServiceImp(repository);

        // Act
        var actual = await service.GetTicketInAllRequests();

        // Assert
        AssertExtension.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(CheckStatusTicketForInsertAutoVoidQueueTestData))]
    public async Task CheckStatusTicketForInsertAutoVoidQueue_ShouldReturnExpectedData(int siteDbGroupId,
        IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets,
        IEnumerable<RequestDetailTicketModel> cricketTickets)
    {
        // Arrange
        var fakeRequests = new List<RequestModel> { new() { RequestId = 112 } };
        var validSingleTickets = new List<RequestDetailTicketModel> { new() };
        var validParlayTickets = new List<RequestDetailTicketModel> { new() };
        var validCricketTickets = new List<RequestDetailTicketModel> { new() };
        var repository = Substitute.For<IDynamicRepository>();
        IEnumerable<object>[] fakeData = { fakeRequests, validSingleTickets, validParlayTickets, validCricketTickets };

        repository.FetchMultipleAsync(default, default)
            .ReturnsForAnyArgs(fakeData);
        var service = new RequestTicketServiceImp(repository);

        // Act
        var actual =
            await service.CheckStatusTicketForInsertAutoVoidQueue(siteDbGroupId, singleTickets, parlayTickets, cricketTickets);

        // Assert
        Assert.StrictEqual(fakeRequests, actual.ValidRequests);
    }

    [Theory]
    [MemberData(nameof(UpdateIsProcessedTestData))]
    public async Task UpdateIsProcessed_ShouldExecuteRepositorySuccessful(IEnumerable<RequestDetailTicketModel> singleTickets,
        IEnumerable<RequestDetailTicketModel> parlayTickets, IEnumerable<RequestDetailTicketModel> cricketTickets, IEnumerable<RequestDetailTicketModel> singleTickets14)
    {
        // Arrange
        var repository = Substitute.For<IDynamicRepository>();
        var counter = 0;
        repository.When(x => x.ExecuteAsync(Arg.Any<UpdateIsProcessedForRequestTicketsCriteria>()))
            .Do(x => counter++);
        var service = new RequestTicketServiceImp(repository);

        // Act
        await service.UpdateIsProcessed(singleTickets, parlayTickets, cricketTickets, singleTickets14);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Any<UpdateIsProcessedForRequestTicketsCriteria>());
        Assert.Equal(1, counter);
    }

    public static IEnumerable<object[]> CheckStatusTicketForInsertAutoVoidQueueTestData =>
        new List<object[]>
        {
            new object[] { 1, null, null, null },
            new object[]
            {
                2, new List<RequestDetailTicketModel> {new() { TransId = 113 }},
                new List<RequestDetailTicketModel>{new() { TransId = 222 } },
                new List<RequestDetailTicketModel> {new() { TransId = 113 }}
            },
        };

    public static IEnumerable<object[]> UpdateIsProcessedTestData =>
        new List<object[]>
        {
            new object[]
            {
                new List<RequestDetailTicketModel> {new() { TransId = 113 }},
                new List<RequestDetailTicketModel> {new() { TransId = 222 }},
                new List<RequestDetailTicketModel> {new() { TransId = 223 }},
                new List<RequestDetailTicketModel> {new() { TransId = 223 }}
            },
        };
}