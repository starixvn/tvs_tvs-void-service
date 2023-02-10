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
        var fakeVoidQueueResults = new List<TicketVoidingQueueModel> { new() { RequestId = 1 }, new() { RequestId = 22 } };
        var privateService = Substitute.For<InsertAfterVoidDataPrivateService>();
        var requestDetailService = Substitute.For<RequestDetailService>();
        requestDetailService.GetRequestById(default)
            .ReturnsForAnyArgs((new List<RequestModel> { new() { RequestId = 112 } }, new List<RequestDetail>()));
        privateService.IsAutoVoid(default, default).ReturnsForAnyArgs(true);
        var processStage = new InsertAfterVoidDataStageImp(requestDetailService, privateService);

        // Act
        await processStage.Execute(fakeVoidQueueResults, new List<string>());

        // Assert
        await requestDetailService.ReceivedWithAnyArgs(1).GetRequestById(default);
        await requestDetailService.ReceivedWithAnyArgs(1).InsertAfterVoidServiceData(default, default);
    }
}