using NSubstitute;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Tests.Services.ScanAutoVoidRequest
{
    public class CheckRequestQueueValidStageImpTests
    {
        [Fact]
        public async Task Execute_RequestValid_ReturnTrue()
        {
            const int siteDbGroupId = 0;
            var queue = new AutoVoidRequestQueueModel
            {
                Action = "CAL"
            };
            var requests = new List<RequestModel>
            {
                new()
                {
                    ActionName = "CAL"
                }
            };
            var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
            var requestDetailService = Substitute.For<RequestDetailService>();
            requestDetailService.GetRequestById(default).ReturnsForAnyArgs((requests, null));
            var service = new CheckRequestQueueValidStageImp(autoVoidRequestQueueService, requestDetailService);

            var actual = await service.Execute(siteDbGroupId, queue);

            await requestDetailService.Received(1).GetRequestById(Arg.Any<int>());
            Assert.True(actual);
        }

        [Fact]
        public async Task Execute_RequestInvalid_ReturnFalse()
        {
            const int siteDbGroupId = 0;
            var queue = new AutoVoidRequestQueueModel
            {
                Action = "CAL"
            };
            var requests = new List<RequestModel>();
            var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
            var requestDetailService = Substitute.For<RequestDetailService>();
            requestDetailService.GetRequestById(default).ReturnsForAnyArgs((requests, null));
            var service = new CheckRequestQueueValidStageImp(autoVoidRequestQueueService, requestDetailService);

            var actual = await service.Execute(siteDbGroupId, queue);

            await requestDetailService.Received(1).GetRequestById(Arg.Any<int>());
            Assert.False(actual);
        }

        [Fact]
        public async Task Execute_RequestWasEdited_ReturnFalse()
        {
            const int siteDbGroupId = 0;
            var queue = new AutoVoidRequestQueueModel
            {
                Action = "CAL"
            };
            var requests = new List<RequestModel>
            {
                new()
                {
                    ActionName = "CAL-HT"
                }
            };
            var autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
            var requestDetailService = Substitute.For<RequestDetailService>();
            requestDetailService.GetRequestById(default).ReturnsForAnyArgs((requests, null));
            var service = new CheckRequestQueueValidStageImp(autoVoidRequestQueueService, requestDetailService);

            var actual = await service.Execute(siteDbGroupId, queue);

            await requestDetailService.Received(1).GetRequestById(Arg.Any<int>());
            await autoVoidRequestQueueService.Received(1).UpdateQueue(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<string>());
            Assert.False(actual);
        }
    }
}