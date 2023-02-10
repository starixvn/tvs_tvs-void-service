using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Criteria.MultiSiteDb;
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
        var fakeSingleTickets = new List<RequestDetailTicketModel> { new() { TransId = 113 } };
        var fakeParlayTickets = new List<RequestDetailTicketModel> { new() { TransId = 222 } };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel>(default).ReturnsForAnyArgs((fakeSingleTickets, fakeParlayTickets));
        var service = new RequestTicketServiceImp(repository);

        // Act
        var actual = await service.GetTicketsByRequestsAndSiteDbGroupId(siteDbGroupId, requestIds);

        // Assert
        await repository.Received(1).FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel>
            (Arg.Is<GetRequestTicketsCriteria>(x => x.SiteDbGroupId == siteDbGroupId && x.RequestIds == requestIds));
        Assert.StrictEqual((fakeSingleTickets, fakeParlayTickets), actual);
    }

    [Fact]
    public async Task GetTicketInAllRequests_ShouldReturnExpectedData()
    {
        // Arrange
        var fakeSingleTickets = new List<RequestDetailTicketModel> { new() { TransId = 113 } };
        var fakeParlayTickets = new List<RequestDetailTicketModel> { new() { TransId = 222 } };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel>(default).ReturnsForAnyArgs((fakeSingleTickets, fakeParlayTickets));
        var service = new RequestTicketServiceImp(repository);

        // Act
        var actual = await service.GetTicketInAllRequests();

        // Assert
        await repository.Received(1).FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel>
            (Arg.Is<GetRequestTicketsCriteria>(x => x.SiteDbGroupId == null && x.RequestIds == null));
        Assert.StrictEqual((fakeSingleTickets, fakeParlayTickets), actual);
    }

    [Theory]
    [MemberData(nameof(CheckStatusTicketForInsertAutoVoidQueueTestData))]
    public async Task CheckStatusTicketForInsertAutoVoidQueue_ShouldReturnExpectedData(int siteDbGroupId,
        IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
    {
        // Arrange
        var fakeRequests = new List<RequestModel> { new() { RequestId = 112 } };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchAsync<RequestModel>(default).ReturnsForAnyArgs(fakeRequests);
        var service = new RequestTicketServiceImp(repository);

        // Act
        var actual =
            await service.CheckStatusTicketForInsertAutoVoidQueue(siteDbGroupId, singleTickets, parlayTickets);

        // Assert
        await repository.Received(1).FetchAsync<RequestModel>
            (Arg.Is<CheckStatusTicketCriteria>(x => x.SiteDbGroupId == siteDbGroupId));
        Assert.StrictEqual(fakeRequests, actual);
    }

    [Theory]
    [MemberData(nameof(UpdateIsProcessedTestData))]
    public async Task UpdateIsProcessed_ShouldExecuteRepositorySuccessful(IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
    {
        // Arrange
        var repository = Substitute.For<IDynamicRepository>();
        var counter = 0;
        repository.When(x => x.ExecuteAsync(Arg.Any<UpdateIsProcessedForRequestTicketsCriteria>()))
            .Do(x => counter++);
        var service = new RequestTicketServiceImp(repository);

        // Act
        await service.UpdateIsProcessed(singleTickets, parlayTickets);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Any<UpdateIsProcessedForRequestTicketsCriteria>());
        Assert.Equal(1, counter);
    }

    public static IEnumerable<object[]> CheckStatusTicketForInsertAutoVoidQueueTestData =>
        new List<object[]>
        {
            new object[] { 1, null, null },
            new object[] { 2, new List<RequestDetailTicketModel> {new() { TransId = 113 }}, new List<RequestDetailTicketModel>{new() { TransId = 222 } }},
        };

    public static IEnumerable<object[]> UpdateIsProcessedTestData =>
        new List<object[]>
        {
            new object[] {  null, null },
            new object[] {  new List<RequestDetailTicketModel> {new() { TransId = 113 }}, new List<RequestDetailTicketModel>{new() { TransId = 222 } }},
        };
}