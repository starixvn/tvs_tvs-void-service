using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Tests.Services;

public class NotificationServiceImpTests
{
    [Theory]
    [InlineData(123)]
    [InlineData(0)]
    public async Task NotifyRequestRunningToAllUser_ShouldExecuteSuccessful(int requestId)
    {
        // Arrange
        var hubContext = Substitute.For<IHubContext<NotificationHub, ClientEvent>>();
        var hubClient = Substitute.For<IHubClients<ClientEvent>>();
        var clientEvent = Substitute.For<ClientEvent>();
        hubClient.All.ReturnsForAnyArgs(clientEvent);
        hubContext.Clients.ReturnsForAnyArgs(hubClient);

        var service = new SignalRServiceImp(hubContext, default);

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
        var loggingService = Substitute.For<LoggingService>();
        var service = new SignalRServiceImp(default, loggingService);

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
    public async Task NotifyVoidFailedToUser_ShouldExecuteSuccessful(int requestId, int voidingTypeId, int statusId)
    {
        // Arrange
        var voidQueueResults = new List<TicketVoidingQueueModel> { new() { UserName = "abc", VoidingTypeId = voidingTypeId, StatusId = statusId } };
        var hubContext = Substitute.For<IHubContext<NotificationHub, ClientEvent>>();
        var hubClient = Substitute.For<IHubClients<ClientEvent>>();
        var clientEvent = Substitute.For<ClientEvent>();
        hubClient.User(Arg.Any<string>()).ReturnsForAnyArgs(clientEvent);
        hubContext.Clients.ReturnsForAnyArgs(hubClient);

        var service = new SignalRServiceImp(hubContext, default);

        // Act
        await service.UpdateVoidFailedToUser(requestId, voidQueueResults);

        // Assert
        hubClient.Received(1).User(Arg.Is<string>(x => x == "abc"));
        await clientEvent.Received(1)
            .showErrorMessage(Arg.Any<string>());
    }

    [Theory]
    [MemberData(nameof(NotifyVoidFailedToUserThrowExceptionTestData))]
    public async Task NotifyVoidFailedToUser_ShouldExecuteCatchExceptionBlock(int requestId, IList<TicketVoidingQueueModel> voidQueueResults)
    {
        // Arrange
        var loggingService = Substitute.For<LoggingService>();
        var service = new SignalRServiceImp(default, loggingService);

        // Act
        await service.UpdateVoidFailedToUser(requestId, voidQueueResults);

        // Assert
        await loggingService.Received(1).ErrorAsync(Arg.Any<Exception>());
    }

    [Theory]
    [InlineData(123)]
    [InlineData(0)]
    public async Task NotifyRequestFailedToAllUser_ShouldExecuteSuccessful(int requestId)
    {
        // Arrange
        var hubContext = Substitute.For<IHubContext<NotificationHub, ClientEvent>>();
        var hubClient = Substitute.For<IHubClients<ClientEvent>>();
        var clientEvent = Substitute.For<ClientEvent>();
        hubClient.All.ReturnsForAnyArgs(clientEvent);
        hubContext.Clients.ReturnsForAnyArgs(hubClient);

        var service = new SignalRServiceImp(hubContext, default);

        // Act
        await service.UpdateRequestFailedToAllUser(requestId);

        // Assert
        await clientEvent.Received(1)
            .updateRequestStatus(Arg.Is<IEnumerable<RequestStatus>>(y => y.First().RequestId == requestId));
    }

    [Theory]
    [InlineData(123)]
    [InlineData(0)]
    public async Task NotifyRequestFailedToAllUser_ShouldExecuteCatchExceptionBlock(int requestId)
    {
        // Arrange
        var loggingService = Substitute.For<LoggingService>();
        var service = new SignalRServiceImp(default, loggingService);

        // Act
        await service.UpdateRequestFailedToAllUser(requestId);

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