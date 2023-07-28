using NSubstitute;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Tests.Services.Void;

public class InsertAfterVoidDataStageImpTests
{
    [Fact]
    public async Task Execute_ShouldExecuteSuccessful()
    {
        // Arrange
        var fakeVoidQueueResults = new List<TicketVoidingQueueModel>
            { new() { RequestId = 1, VoidingHistoryId = 1 }, new() { RequestId = 22 } };
        var privateService = Substitute.For<InsertAfterVoidDataPrivateService>();
        var requestDetailService = Substitute.For<RequestDetailService>();
        requestDetailService.GetRequestById(default)
            .ReturnsForAnyArgs((new List<RequestModel> { new() { RequestId = 112 } }, new List<RequestDetail>()));
        requestDetailService.GetVoidDataOfVoidingHistory(default)
            .ReturnsForAnyArgs(new VoidDataXmlModel { IsRunAutoAfterVoid = true });
        requestDetailService.InsertAfterVoidServiceData(default, Arg.Any<AfterVoidServiceModel>())
            .ReturnsForAnyArgs(new AfterVoidServiceModel());
        privateService.GetMatchIdForAfterVoid(default, Arg.Any<IEnumerable<RequestDetail>>(),
                Arg.Any<IEnumerable<TicketVoidingQueueModel>>())
            .ReturnsForAnyArgs(1);
        privateService.CombineNegativeBalanceUsers(Arg.Any<IEnumerable<TicketVoidingQueueModel>>())
            .ReturnsForAnyArgs("1");

        var processStage = new InsertAfterVoidDataStageImp(requestDetailService, privateService);

        // Act
        await processStage.Execute(fakeVoidQueueResults, new List<string>());

        // Assert
        await requestDetailService.Received(1).GetRequestById(1);
        await requestDetailService.Received(1).GetVoidDataOfVoidingHistory(1);
        await requestDetailService.Received(1).InsertAfterVoidServiceData(true, Arg.Any<AfterVoidServiceModel>());
        privateService.Received(1).GetMatchIdForAfterVoid(default, Arg.Any<IEnumerable<RequestDetail>>(),
            Arg.Any<IEnumerable<TicketVoidingQueueModel>>());
        privateService.Received(1).CombineNegativeBalanceUsers(Arg.Any<IEnumerable<TicketVoidingQueueModel>>());
    }
}