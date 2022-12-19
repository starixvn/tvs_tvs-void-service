using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Tests.Services
{
    public class RequestDetailServiceImpTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        public async Task InsertVoidingHistory_ShouldReturnExpectedData(int requestId)
        {
            // Arrange
            var fakeVoidingHistory = new VoidingHistoryModel { VoidingHistoryId = 112 };
            var repository = Substitute.For<IDynamicRepository>();
            repository.GetAsync<VoidingHistoryModel>(default).ReturnsForAnyArgs(fakeVoidingHistory);
            var service = new RequestDetailServiceImp(repository);

            // Act
            var actual = await service.InsertVoidingHistory(requestId);

            // Assert
            await repository.Received(1).GetAsync<VoidingHistoryModel>
            (Arg.Is<InsertVoidingHistoryCriteria>(x =>
                x.RequestId == requestId));
            Assert.StrictEqual(fakeVoidingHistory, actual);
        }

        [Theory]
        [InlineData(true, 112)]
        [InlineData(false, 223)]
        public async Task InsertAfterVoidServiceData_ShouldReturnExpectedData(bool isAutoVoid, int requestId)
        {
            // Arrange
            var fakeAfterVoid = new AfterVoidServiceModel { VoidingHistoryId = 112 };
            var repository = Substitute.For<IDynamicRepository>();
            repository.GetAsync<AfterVoidServiceModel>(default).ReturnsForAnyArgs(fakeAfterVoid);
            var service = new RequestDetailServiceImp(repository);

            // Act
            var actual = await service.InsertAfterVoidServiceData(isAutoVoid, new AfterVoidServiceModel() { RequestId = requestId });

            // Assert
            await repository.Received(1).GetAsync<AfterVoidServiceModel>
            (Arg.Is<InsertAfterVoidServiceDataCriteria>(x => x.IsAutoVoid == isAutoVoid &&
                x.RequestId == requestId));
            Assert.StrictEqual(fakeAfterVoid, actual);
        }

        [Theory]
        [InlineData(112, 1)]
        [InlineData(0, 0)]
        [InlineData(222, 0)]
        [InlineData(0, 2)]
        public async Task UpdateTicketVoidingHistory_ShouldExecuteRepositorySuccessful(int voidingHistoryId, int statusId)
        {
            // Arrange
            var repository = Substitute.For<IDynamicRepository>();
            var counter = 0;
            repository.When(x => x.ExecuteAsync(Arg.Any<UpdateTicketVoidingHistoryCriteria>()))
                .Do(x => counter++);
            var service = new RequestDetailServiceImp(repository);

            // Act
            await service.UpdateTicketVoidingHistory(voidingHistoryId, statusId);

            // Assert
            await repository.Received(1).ExecuteAsync(Arg.Any<UpdateTicketVoidingHistoryCriteria>());
            Assert.Equal(1, counter);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(0)]
        public async Task GetRequestById_ShouldReturnExpectedData(int requestId)
        {
            // Arrange
            var fakeRequests = new List<RequestModel> { new() { RequestId = 113 } };
            var fakeRequestsDetail = new List<RequestDetail> { new() { RequestId = 222 } };
            var repository = Substitute.For<IDynamicRepository>();
            repository.FetchMultipleAsync<RequestModel, RequestDetail>(default).ReturnsForAnyArgs((fakeRequests, fakeRequestsDetail));
            var service = new RequestDetailServiceImp(repository);

            // Act
            var actual = await service.GetRequestById(requestId);

            // Assert
            await repository.Received(1).FetchMultipleAsync<RequestModel, RequestDetail>
                (Arg.Is<GetRequestCriteria>(x => x.RequestId == requestId));
            Assert.StrictEqual((fakeRequests, fakeRequestsDetail), actual);
        }
    }
}