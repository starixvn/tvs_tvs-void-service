using Fanex.Data.Repository;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Tests.Services;

public class RequestDetailServiceTests
{
    [Theory]
    [InlineData("2,3", 1, "123xyz")]
    [InlineData("1", 0, "")]
    public async Task InsertVoidingHistory_ShouldReturnExpectedData(string requestIds, int siteDbGroupId,
        string broadcastSessionId)
    {
        // Arrange
        var fakeVoidingHistory = new List<VoidingHistoryModel> { new() { VoidingHistoryId = 112 } };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchAsync<VoidingHistoryModel>(default).ReturnsForAnyArgs(fakeVoidingHistory);
        const string voidDataXml = "xml";
        var service = new RequestDetailServiceImp(repository, null);

        // Act
        var actual = await service.InsertVoidingHistory(requestIds, siteDbGroupId, broadcastSessionId, voidDataXml);

        // Assert
        await repository.Received(1).FetchAsync<VoidingHistoryModel>(Arg.Any<InsertVoidingHistoryCriteria>());
        AssertExtension.Equal(fakeVoidingHistory, actual);
    }

    [Theory]
    [InlineData(true, 112)]
    [InlineData(false, 223)]
    public async Task InsertAfterVoidServiceData_ShouldCallFanexData(bool isAutoVoid, int requestId)
    {
        // Arrange
        var fakeAfterVoid = new AfterVoidServiceModel { VoidingHistoryId = 112 };
        var repository = Substitute.For<IDynamicRepository>();
        repository.GetAsync<AfterVoidServiceModel>(default).ReturnsForAnyArgs(fakeAfterVoid);
        var service = new RequestDetailServiceImp(repository, null);

        // Act
        await service.InsertAfterVoidServiceData(isAutoVoid, new AfterVoidServiceModel { RequestId = requestId }, new List<string>());

        // Assert
        await repository.Received(1).GetAsync<AfterVoidServiceModel>
        (Arg.Is<InsertAfterVoidServiceDataCriteria>(x => x.IsAutoVoid == isAutoVoid &&
                                                         x.RequestId == requestId));
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
            .Do(_ => counter++);
        var service = new RequestDetailServiceImp(repository, null);

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
        repository.FetchMultipleAsync<RequestModel, RequestDetail>(default)
            .ReturnsForAnyArgs((fakeRequests, fakeRequestsDetail));
        var service = new RequestDetailServiceImp(repository, null);

        // Act
        var actual = await service.GetRequestById(requestId);

        // Assert
        await repository.Received(1).FetchMultipleAsync<RequestModel, RequestDetail>
            (Arg.Is<GetRequestCriteria>(x => x.RequestId == requestId));
        Assert.StrictEqual((fakeRequests, fakeRequestsDetail), actual);
    }

    [Fact]
    public async Task GetVoidDataOfVoidingHistory_ValidData_ReturnExpected()
    {
        // Arrange
        var voidingHistory = new VoidDataXmlModel { VoidingHistoryId = 1 };
        var voidingHistories = new List<VoidDataXmlModel> { voidingHistory };
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchAsync<VoidDataXmlModel>(Arg.Any<GetVoidDataOfVoidingHistoryCriteria>())
            .ReturnsForAnyArgs(voidingHistories);
        var service = new RequestDetailServiceImp(repository, null);

        // Act
        var actual = await service.GetVoidDataOfVoidingHistory(1);

        // Assert
        await repository.Received(1).FetchAsync<VoidDataXmlModel>(Arg.Any<GetVoidDataOfVoidingHistoryCriteria>());
        AssertExtension.Equal(voidingHistory, actual);
    }

    [Fact]
    public async Task InsertLetstalk_IsArchivedTicket_ShouldReturn()
    {
        // Arrange
        var repository = Substitute.For<IDynamicRepository>();
        repository.ExecuteAsync(default).ReturnsForAnyArgs(1);
        var service = new RequestDetailServiceImp(repository, null);

        // Act
        await service.InsertLetsTalk(1, IssueTypeConstant.ArchivedTicket, "", 0, new List<string>());

        // Assert
        await repository.DidNotReceive().ExecuteAsync(Arg.Any<InsertLetstalkCommand>());
    }

    [Fact]
    public async Task InsertLetstalk_IsDangerAccepted_ShouldCalledFanexData()
    {
        // Arrange
        var logData = new List<string>();
        var repository = Substitute.For<IDynamicRepository>();
        repository.ExecuteAsync(default).ReturnsForAnyArgs(1);
        var service = new RequestDetailServiceImp(repository, null);

        // Act
        await service.InsertLetsTalk(1, IssueTypeConstant.DangerAccepted, "", 0, logData);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Any<InsertLetstalkCommand>());
        Assert.Contains("InsertLetsTalk: Done", logData);
    }

    [Fact]
    public async Task InsertLetstalk_HasException_ShouldCalledLoggingServiceErrorAsync()
    {
        // Arrange
        var exception = new Exception("Test");
        var logData = new List<string>();
        var repository = Substitute.For<IDynamicRepository>();
        var loggingService = Substitute.For<LoggingService>();
        repository.ExecuteAsync(Arg.Any<InsertLetstalkCommand>())
            .ThrowsAsync(exception);
        var service = new RequestDetailServiceImp(repository, loggingService);

        // Act
        await service.InsertLetsTalk(1, IssueTypeConstant.DangerAccepted, "", 0, logData);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Any<InsertLetstalkCommand>());
        await loggingService.Received(1).ErrorAsync(exception);
        Assert.Contains("InsertLetsTalk: Failed", logData);
    }
}