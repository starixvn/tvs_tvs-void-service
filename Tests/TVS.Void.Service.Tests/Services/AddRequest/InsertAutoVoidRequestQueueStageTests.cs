using NSubstitute;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.AddRequest;

namespace TVS.Void.Service.Tests.Services.AddRequest;

public class InsertAutoVoidRequestQueueStageTests
{
    private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
    private readonly InsertAutoVoidRequestQueueStageImp service;

    public InsertAutoVoidRequestQueueStageTests()
    {
        autoVoidRequestQueueService = Substitute.For<AutoVoidRequestQueueService>();
        service = new InsertAutoVoidRequestQueueStageImp(autoVoidRequestQueueService);
    }

    [Fact]
    public async Task Execute_ValidData_ReturnExpected()
    {
        // Arrange
        var validation = new CheckDuplicatedTicketsResult { SiteDBAffected = "1" };
        var logData = new List<string> { "123" };

        // Act
        await service.Execute(123, validation, logData);

        // Assert
        await autoVoidRequestQueueService.Received(1).InsertQueue(1, Arg.Any<string>(), "123",
            ExecuteStatus.New, AutoVoidConstant.QueueActionVoidInstant, null);
        Assert.Equal(2, logData.Count);
    }
}