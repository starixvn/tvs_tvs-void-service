using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class CheckVoidWinningRequestQueueValidStageTests
{
    private RequestDetailService requestDetailService = null!;
    private AutoVoidRequestQueueService autoVoidRequestQueueService = null!;
    private CheckVoidWinningRequestQueueValidStageImp service = null!;

    [Fact]
    public async Task Execute_QueueHasRequestsAllChangeAction_ReturnExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        const int siteDbGroupId = 0;
        string[] requestIdList = ["1", "2"];
        requestDetailService = Substitute.For<RequestDetailService>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        foreach (var requestId in requestIdList)
        {
            var request = new List<RequestModel> { new() { RequestId = int.Parse(requestId), ActionName = "VoidInstant" } };
            requestDetailService.GetRequestById(int.Parse(requestId)).Returns((request, null));
        }

        const string queueErrorMessage = "Requests (1,2) Action was edited.";
        service = new CheckVoidWinningRequestQueueValidStageImp(requestDetailService, autoVoidRequestQueueService);

        // Act
        var (isValid, requestIdsChangedAction) = await service.Execute(siteDbGroupId, queue);

        // Assert
        await requestDetailService.Received(2).GetRequestById(Arg.Any<int>());
        await autoVoidRequestQueueService.Received(1).UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
        Assert.False(isValid);
        Assert.Equal(string.Empty, requestIdsChangedAction);
    }

    [Fact]
    public async Task Execute_QueueHasRequestChangeAction_ReturnExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        const int siteDbGroupId = 0;
        requestDetailService = Substitute.For<RequestDetailService>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var request = new List<RequestModel> { new() { RequestId = 1, ActionId = 4 } };
        requestDetailService.GetRequestById(1).Returns((request, null));
        var request2 = new List<RequestModel> { new() { RequestId = 2, ActionId = 1 } };
        requestDetailService.GetRequestById(2).Returns((request2, null));
        service = new CheckVoidWinningRequestQueueValidStageImp(requestDetailService, autoVoidRequestQueueService);

        // Act
        var (isValid, requestIdsChangedAction) = await service.Execute(siteDbGroupId, queue);

        // Assert
        await requestDetailService.Received(2).GetRequestById(Arg.Any<int>());
        Assert.True(isValid);
        Assert.Equal("1", requestIdsChangedAction);
    }

    [Fact]
    public async Task Execute_QueueHasRequestsNotChangeAction_ReturnExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        const int siteDbGroupId = 0;
        string[] requestIdList = ["1", "2"];
        requestDetailService = Substitute.For<RequestDetailService>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        foreach (var requestId in requestIdList)
        {
            var request = new List<RequestModel> { new() { RequestId = int.Parse(requestId), ActionId = 1 } };
            requestDetailService.GetRequestById(int.Parse(requestId)).Returns((request, null));
        }

        service = new CheckVoidWinningRequestQueueValidStageImp(requestDetailService, autoVoidRequestQueueService);

        // Act
        var (isValid, requestIdsChangedAction) = await service.Execute(siteDbGroupId, queue);

        // Assert
        await requestDetailService.Received(2).GetRequestById(Arg.Any<int>());
        Assert.True(isValid);
        Assert.Equal(string.Empty, requestIdsChangedAction);
    }

    [Fact]
    public async Task Execute_NotSuccessGetRequestById_ReturnExpected()
    {
        // Arrange
        var queue = new AutoVoidRequestQueueModel
        {
            QueueId = 1,
            RequestIds = "1,2",
            Action = "CAL"
        };
        const int siteDbGroupId = 0;
        string[] requestIdList = ["1", "2"];
        requestDetailService = Substitute.For<RequestDetailService>();
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        foreach (var requestId in requestIdList)
        {
            requestDetailService.GetRequestById(int.Parse(requestId)).Returns((new List<RequestModel>(), null));
        }

        service = new CheckVoidWinningRequestQueueValidStageImp(requestDetailService, autoVoidRequestQueueService);

        // Act
        var (isValid, requestIdsChangedAction) = await service.Execute(siteDbGroupId, queue);

        // Assert
        await requestDetailService.Received(2).GetRequestById(Arg.Any<int>());
        Assert.True(isValid);
        Assert.Equal(string.Empty, requestIdsChangedAction);
    }
}