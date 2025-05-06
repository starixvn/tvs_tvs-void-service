using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.Notification;

namespace TVS.Void.Service.Tests.Services;

public class NotificationServiceImpTests
{
    private readonly IHubContext<NotificationHub, ClientEvent> hubContext = Substitute.For<IHubContext<NotificationHub, ClientEvent>>();
    private readonly IHubClients<ClientEvent> hubClient = Substitute.For<IHubClients<ClientEvent>>();
    private readonly ClientEvent clientEvent = Substitute.For<ClientEvent>();
    private readonly LoggingService loggingService = Substitute.For<LoggingService>();
    private readonly NotificationService notificationService = Substitute.For<NotificationService>();

    [Theory]
    [InlineData(123)]
    [InlineData(0)]
    public async Task NotifyRequestRunningToAllUser_ShouldExecuteSuccessful(int requestId)
    {
        // Arrange
        hubClient.All.ReturnsForAnyArgs(clientEvent);
        hubContext.Clients.ReturnsForAnyArgs(hubClient);

        var service = new SignalRServiceImp(hubContext, loggingService, notificationService);

        // Act
        await service.UpdateRequestRunningToAllUser(new List<VoidingHistoryModel> { new() { RequestId = requestId } });

        // Assert
        await clientEvent.Received(1)
            .updateRequestStatus(Arg.Is<IEnumerable<RequestStatus>>(y => y.First().RequestId == requestId));
    }

    [Theory]
    [InlineData(123)]
    [InlineData(0)]
    public async Task NotifyRequestRunningToAllUser_ShouldExecuteCatchExceptionBlock(int requestId)
    {
        // Arrange
        hubContext.Clients.All.When(x => x.updateRequestStatus(Arg.Any<IEnumerable<RequestStatus>>()))
            .Do(x => throw new Exception());
        var service = new SignalRServiceImp(hubContext, loggingService, notificationService);

        // Act
        await service.UpdateRequestRunningToAllUser(new List<VoidingHistoryModel> { new() { RequestId = requestId } });

        // Assert
        await loggingService.Received(1).ErrorAsync(Arg.Any<Exception>());
    }

    [Theory]
    [InlineData(123, VoidingTypeConstant.Cricket, ExecuteStatus.Failed)]
    [InlineData(123, VoidingTypeConstant.CancelCashOut, ExecuteStatus.Failed)]
    [InlineData(0, VoidingTypeConstant.Cricket, ExecuteStatus.Done)]
    [InlineData(0, VoidingTypeConstant.CancelCashOut, ExecuteStatus.Done)]
    public void NotifyVoidFailedToUser_ShouldExecuteSuccessful(int requestId, byte voidingTypeId, int statusId)
    {
        // Arrange
        var voidQueueResults = new List<TicketVoidingQueueModel> { new() { UserName = "abc", VoidingTypeId = voidingTypeId, StatusId = statusId } };
        hubClient.User(Arg.Any<string>()).ReturnsForAnyArgs(clientEvent);
        hubContext.Clients.ReturnsForAnyArgs(hubClient);

        var service = new SignalRServiceImp(hubContext, loggingService, notificationService);

        // Act
        service.UpdateVoidFailedToUser(requestId, voidQueueResults);

        // Assert
        notificationService.Received(1).ExecuteBackground(Arg.Any<Func<Task>>());
    }

    [Theory]
    [InlineData(123)]
    [InlineData(0)]
    public async Task NotifyRequestFailedToAllUser_ShouldExecuteSuccessful(int requestId)
    {
        // Arrange
        hubClient.All.ReturnsForAnyArgs(clientEvent);
        hubContext.Clients.ReturnsForAnyArgs(hubClient);

        var service = new SignalRServiceImp(hubContext, loggingService, notificationService);

        // Act
        await service.UpdateRequestFailedToAllUser(requestId);

        // Assert
        await clientEvent.Received(1)
            .updateRequestStatus(Arg.Is<IEnumerable<RequestStatus>>(y => y.First().RequestId == requestId));
    }

    [Fact]
    public async Task NotifyRequestFailedToAllUser_ShouldExecuteCatchExceptionBlock()
    {
        // Arrange
        hubContext.Clients.All.When(x => x.updateRequestStatus(Arg.Any<IEnumerable<RequestStatus>>()))
            .Do(x => throw new Exception());
        var service = new SignalRServiceImp(hubContext, loggingService, notificationService);

        // Act
        await service.UpdateRequestFailedToAllUser(1);

        // Assert
        await loggingService.Received(1).ErrorAsync(Arg.Any<Exception>());
    }

    public static IEnumerable<object?[]> NotifyVoidFailedToUserThrowExceptionTestData =>
        new List<object?[]>
        {
            new object?[] {  123, new List<TicketVoidingQueueModel>() },
            new object?[] {  0, null}
        };
}