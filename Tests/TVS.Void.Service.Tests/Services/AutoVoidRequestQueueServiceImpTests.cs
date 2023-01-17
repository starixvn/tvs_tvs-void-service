using Fanex.Data.Repository;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Tests.Services
{
    public class AutoVoidRequestQueueServiceImpTests
    {
        [Fact]
        public async Task GetQueue_ShouldReturnNullData()
        {
            // Arrange
            const int fakeSiteDbGroupId = 1;
            var repository = Substitute.For<IDynamicRepository>();
            repository.GetAsync<AutoVoidRequestQueueModel>(default).ReturnsNullForAnyArgs();
            var service = new AutoVoidRequestQueueServiceImp(repository);

            // Act
            var actual = await service.GetQueue(fakeSiteDbGroupId);

            // Assert
            await repository.Received(1).GetAsync<AutoVoidRequestQueueModel>
            (Arg.Is<GetAutoVoidRequestQueueCriteria>(x => x.SiteDbGroupId == fakeSiteDbGroupId));
            Assert.Null(actual);
        }

        [Fact]
        public async Task GetQueue_ShouldReturnExpectedData()
        {
            // Arrange
            const int fakeSiteDbGroupId = 113;
            var fakeAutoVoidRequestQueue = new AutoVoidRequestQueueModel() { QueueId = 22222222 };
            var repository = Substitute.For<IDynamicRepository>();
            repository.GetAsync<AutoVoidRequestQueueModel>(default).ReturnsForAnyArgs(fakeAutoVoidRequestQueue);
            var service = new AutoVoidRequestQueueServiceImp(repository);

            // Act
            var actual = await service.GetQueue(fakeSiteDbGroupId);

            // Assert
            await repository.Received(1).GetAsync<AutoVoidRequestQueueModel>
                (Arg.Is<GetAutoVoidRequestQueueCriteria>(x => x.SiteDbGroupId == fakeSiteDbGroupId));
            Assert.Equal(fakeSiteDbGroupId, actual.SiteDbGroupId);
            Assert.Equal(fakeAutoVoidRequestQueue.QueueId, actual.QueueId);
        }

        [Theory]
        [InlineData(1, 2, 3, "abc")]
        [InlineData(0, 0, null, "")]
        public async Task UpdateQueue_ShouldExecuteRepositorySuccessful(int siteDbGroupId, int queueId, int? statusId, string errorMessage)
        {
            // Arrange
            var repository = Substitute.For<IDynamicRepository>();
            var service = new AutoVoidRequestQueueServiceImp(repository);

            // Act
            await service.UpdateQueue(siteDbGroupId, queueId, statusId, errorMessage);

            // Assert
            await repository.Received(1).ExecuteAsync(Arg.Is<UpdateAutoVoidRequestQueueCriteria>(x
                => x.SiteDbGroupId == siteDbGroupId && x.QueueId == queueId && x.StatusId == statusId && x.ErrMsg == errorMessage));
        }

        [Theory]
        [InlineData(1, "xyz", "2,3", 3, "abc", 5)]
        [InlineData(0, "", "1", 0, "", null)]
        public async Task InsertQueue_ShouldExecuteRepositorySuccessful(int siteDbGroupId, string broadcastSessionId, string requestIds, int statusId, string action, int? matchId)
        {
            // Arrange
            var repository = Substitute.For<IDynamicRepository>();
            var service = new AutoVoidRequestQueueServiceImp(repository);

            // Act
            await service.InsertQueue(siteDbGroupId, broadcastSessionId, requestIds, statusId, action, matchId);

            // Assert
            await repository.Received(1).ExecuteAsync(Arg.Is<InsertAutoVoidRequestQueueCriteria>(x
                => x.SiteDbGroupId == siteDbGroupId && x.RequestIds == requestIds && x.StatusId == statusId && x.Action == action && x.MatchId == matchId));
        }
    }
}