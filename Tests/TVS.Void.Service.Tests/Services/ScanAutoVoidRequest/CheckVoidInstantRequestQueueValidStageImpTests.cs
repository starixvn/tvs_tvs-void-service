using NSubstitute;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest;

public class CheckVoidInstantRequestQueueValidStageImpTests
{
    [Fact]
    public async Task Execute_RequestValid_ReturnTrue()
    {
        const int siteDbGroupId = 0;
        var queue = new AutoVoidRequestQueueModel
        {
            RequestIds = "222",
            Action = "CAL"
        };
        var requests = new List<RequestModel>
        {
            new()
            {
                RequestId = 222,
                ActionId = 1
            }
        };
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var requestDetailService = Substitute.For<RequestDetailService>();
        requestDetailService.GetRequestById(default).ReturnsForAnyArgs((requests, null));
        var service = new CheckVoidInstantRequestQueueValidStageImp(autoVoidRequestQueueService, requestDetailService);

        var actual = await service.Execute(siteDbGroupId, queue);

        await requestDetailService.Received(1).GetRequestById(Arg.Any<int>());
        Assert.True(actual.IsValid);
    }

    [Fact]
    public async Task Execute_TooManyRequestIds_ReturnFalse()
    {
        var countUpdateQueue = 0;
        const int siteDbGroupId = 0;
        var queue = new AutoVoidRequestQueueModel
        {
            RequestIds = "11,22",
            Action = "CAL"
        };
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var requestDetailService = Substitute.For<RequestDetailService>();
        autoVoidRequestQueueService
            .WhenForAnyArgs(x => x.UpdateQueue(default, default, default, default))
            .Do(_ => countUpdateQueue++);
        var service = new CheckVoidInstantRequestQueueValidStageImp(autoVoidRequestQueueService, requestDetailService);

        var actual = await service.Execute(siteDbGroupId, queue);

        await requestDetailService.DidNotReceiveWithAnyArgs().GetRequestById(default);
        Assert.Equal(1, countUpdateQueue);
        Assert.False(actual.IsValid);
    }

    [Fact]
    public async Task Execute_RequestInvalid_ReturnFalse()
    {
        const int siteDbGroupId = 0;
        var queue = new AutoVoidRequestQueueModel
        {
            RequestIds = "1",
            Action = "CAL"
        };
        var requests = new List<RequestModel>();
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var requestDetailService = Substitute.For<RequestDetailService>();
        requestDetailService.GetRequestById(default).ReturnsForAnyArgs((requests, null));
        var service = new CheckVoidInstantRequestQueueValidStageImp(autoVoidRequestQueueService, requestDetailService);

        var actual = await service.Execute(siteDbGroupId, queue);

        await requestDetailService.Received(1).GetRequestById(Arg.Any<int>());
        Assert.False(actual.IsValid);
    }

    [Fact]
    public async Task Execute_RequestWasEdited_ReturnFalse()
    {
        const int siteDbGroupId = 0;
        var queue = new AutoVoidRequestQueueModel
        {
            RequestIds = "1",
            Action = "CAL"
        };
        var requests = new List<RequestModel>
        {
            new()
            {
                ActionId = 4
            }
        };
        var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        var requestDetailService = Substitute.For<RequestDetailService>();
        requestDetailService.GetRequestById(default).ReturnsForAnyArgs((requests, null));
        var service = new CheckVoidInstantRequestQueueValidStageImp(autoVoidRequestQueueService, requestDetailService);

        var actual = await service.Execute(siteDbGroupId, queue);

        await requestDetailService.Received(1).GetRequestById(Arg.Any<int>());
        await autoVoidRequestQueueService.Received(1).UpdateQueue(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<string>());
        Assert.False(actual.IsValid);
    }
}