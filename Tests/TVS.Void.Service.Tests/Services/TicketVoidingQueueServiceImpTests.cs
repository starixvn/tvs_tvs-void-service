using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Tests.Services;

public class TicketVoidingQueueServiceImpTests
{
    [Fact]
    public async Task GetQueues_ShouldReturnExpectedData()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        int? fakeVoidingHistoryId = null;
        var fakeQueues = new List<TicketVoidingQueueModel> { new() { RequestId = 113 } };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchAsync<TicketVoidingQueueModel>(default).ReturnsForAnyArgs(fakeQueues);
        var service = new TicketVoidingQueueServiceImp(repository);

        // Act
        var actual = await service.GetQueues(fakeSiteDbGroupId, fakeVoidingHistoryId);

        // Assert
        await repository.Received(1).FetchAsync<TicketVoidingQueueModel>
        (Arg.Is<GetTicketVoidingQueueCriteria>(x =>
            x.SiteDbGroupId == fakeSiteDbGroupId && x.VoidingHistoryId == fakeVoidingHistoryId));
        Assert.True(actual.Any());
    }

    [Fact]
    public async Task InsertQueue_ShouldExecuteRepositorySuccessful()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const int fakeVoidingHistoryId = 2;
        const int fakeRequestId = 3;
        var counter = 0;
        var repository = Substitute.For<IDynamicRepository>();
        repository.When(x => x.ExecuteAsync(Arg.Any<InsertTicketVoidingQueueCriteria>()))
            .Do(x => counter++);
        var service = new TicketVoidingQueueServiceImp(repository);

        // Act
        await service.InsertQueue(fakeSiteDbGroupId, fakeRequestId, fakeVoidingHistoryId, null, null, null);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Is<InsertTicketVoidingQueueCriteria>(x =>
            x.SiteDbGroupId == fakeSiteDbGroupId && x.RequestId == fakeRequestId && x.VoidingHistoryId == fakeVoidingHistoryId));
        Assert.Equal(1, counter);
    }

    [Fact()]
    public async Task InsertQueueForRequests_ShouldExecuteRepositorySuccessful()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        var fakeVoidingHistoryList = new List<VoidingHistoryModel>
        {
            new() { RequestId = 123, VoidingHistoryId = 222 }, new() { RequestId = 234, VoidingHistoryId = 345 }
        };
        var counter = 0;
        var repository = Substitute.For<IDynamicRepository>();
        repository.When(x => x.ExecuteAsync(Arg.Any<InsertTicketVoidingQueueCriteria>()))
            .Do(x => counter++);
        var service = new TicketVoidingQueueServiceImp(repository);

        // Act
        await service.InsertQueueForRequests(fakeSiteDbGroupId, fakeVoidingHistoryList, new List<RequestDetailTicketModel>(), new List<RequestDetailTicketModel>(), new List<RequestDetailTicketModel>());

        // Assert
        await repository.Received(2).ExecuteAsync(Arg.Is<InsertTicketVoidingQueueCriteria>(x => x.SiteDbGroupId == fakeSiteDbGroupId));
        Assert.Equal(fakeVoidingHistoryList.Count, counter);
    }

    [Fact]
    public async Task ProcessVoidQueue_ShouldReturnExpectedData()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const int fakeQueueId = 2;
        var fakeQueue = new TicketVoidingQueueModel { RequestId = 111, MatchId = 222 };
        var repository = Substitute.For<IDynamicRepository>();
        repository.GetAsync<TicketVoidingQueueModel>(default).ReturnsForAnyArgs(fakeQueue);
        var service = new TicketVoidingQueueServiceImp(repository);

        // Act
        var actual = await service.ProcessVoidQueue(fakeSiteDbGroupId, fakeQueueId, true);

        // Assert
        await repository.Received(1).GetAsync<TicketVoidingQueueModel>
        (Arg.Is<ProcessVoidQueueCriteria>(x =>
            x.SiteDbGroupId == fakeSiteDbGroupId && x.QueueId == fakeQueueId));

        Assert.StrictEqual(fakeQueue, actual);
    }
}