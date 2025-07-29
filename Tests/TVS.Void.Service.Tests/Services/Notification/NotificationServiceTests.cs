using Fanex.Data.Repository;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Services.Notification;

namespace TVS.Void.Service.Tests.Services.Notification;

public class NotificationServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly IHubClients<ClientEvent> hubClients;
    private readonly ClientEvent clientEvent;
    private readonly NotificationServiceImp service;

    public NotificationServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        hubClients = Substitute.For<IHubClients<ClientEvent>>();
        clientEvent = Substitute.For<ClientEvent>();
        var hubContext = Substitute.For<IHubContext<NotificationHub, ClientEvent>>();
        hubContext.Clients.Returns(hubClients);

        service = new NotificationServiceImp(repository, hubContext);
    }

    [Fact]
    public void ExecuteBackground_WithNullAction_ShouldNotThrow()
    {
        // Arrange
        Func<Task>? action = null;

        // Act & Assert
        var exception = Record.Exception(() => service.ExecuteBackground(action));
        Assert.Null(exception);
    }

    [Fact]
    public async Task InsertNotification_WithValidParameters_ShouldCreateCorrectCriteria()
    {
        // Arrange
        const string type = NotificationType.Error;
        const int requestId = 123;
        const string content = "Test notification content";

        var expectedNotifications = new List<NotificationModel>
        {
            new() { Id = 1, Content = content, Type = type }
        };
        var expectedUsers = new List<string> { "user1", "user2" };

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((expectedNotifications, expectedUsers));

        // Act
        await service.InsertNotification(type, requestId, content);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(
            Arg.Is<UpdateNotificationCriteria>(criteria =>
                criteria.RequestId == requestId &&
                criteria.Username == "starixIT" &&
                criteria.Content == content &&
                criteria.Type == type));
    }

    [Theory]
    [InlineData(NotificationType.Error, 123, "Error message")]
    [InlineData(NotificationType.Notification, 456, "Info message")]
    [InlineData("CustomType", 789, "Custom message")]
    public async Task InsertNotification_WithDifferentTypes_ShouldCreateCorrectCriteria(string type, int requestId, string content)
    {
        // Arrange
        var expectedNotifications = new List<NotificationModel>
        {
            new() { Id = 1, Content = content, Type = type }
        };
        var expectedUsers = new List<string> { "user1" };

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((expectedNotifications, expectedUsers));

        // Act
        await service.InsertNotification(type, requestId, content);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(
            Arg.Is<UpdateNotificationCriteria>(criteria =>
                criteria.Type == type &&
                criteria.RequestId == requestId &&
                criteria.Content == content));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public async Task InsertNotification_WithEdgeCaseRequestIds_ShouldCreateCorrectCriteria(int requestId)
    {
        // Arrange
        const string type = NotificationType.Error;
        const string content = "Test content";

        var expectedNotifications = new List<NotificationModel>
        {
            new() { Id = 1, Content = content, Type = type }
        };
        var expectedUsers = new List<string> { "user1" };

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((expectedNotifications, expectedUsers));

        // Act
        await service.InsertNotification(type, requestId, content);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(
            Arg.Is<UpdateNotificationCriteria>(criteria => criteria.RequestId == requestId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("Very long content that exceeds normal limits")]
    public async Task InsertNotification_WithEdgeCaseContent_ShouldCreateCorrectCriteria(string? content)
    {
        // Arrange
        const string type = NotificationType.Notification;
        const int requestId = 123;

        var expectedNotifications = new List<NotificationModel>
        {
            new() { Id = 1, Content = content ?? "", Type = type }
        };
        var expectedUsers = new List<string> { "user1" };

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((expectedNotifications, expectedUsers));

        // Act
        await service.InsertNotification(type, requestId, content);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(
            Arg.Is<UpdateNotificationCriteria>(criteria => criteria.Content == content));
    }

    [Fact]
    public async Task InsertNotification_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        const string type = NotificationType.Error;
        const int requestId = 123;
        const string content = "Test content";

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns(Task.FromException<(IEnumerable<NotificationModel>, IEnumerable<string>)>(new InvalidOperationException("Database error")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InsertNotification(type, requestId, content));
    }

    [Fact]
    public async Task InsertNotification_WithEmptyNotifications_ShouldNotCallSignalR()
    {
        // Arrange
        const string type = NotificationType.Error;
        const int requestId = 123;
        const string content = "Test content";

        var emptyNotifications = new List<NotificationModel>();
        var expectedUsers = new List<string> { "user1", "user2" };

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((emptyNotifications, expectedUsers));

        // Act
        await service.InsertNotification(type, requestId, content);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>());
        // SignalR should not be called when notifications are empty
    }

    [Fact]
    public async Task InsertNotification_WithEmptyUsers_ShouldNotCallSignalR()
    {
        // Arrange
        const string type = NotificationType.Error;
        const int requestId = 123;
        const string content = "Test content";

        var expectedNotifications = new List<NotificationModel>
        {
            new() { Id = 1, Content = content, Type = type }
        };
        var emptyUsers = new List<string>();

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((expectedNotifications, emptyUsers));

        // Act
        await service.InsertNotification(type, requestId, content);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>());
        // SignalR should not be called when users are empty
    }

    [Fact]
    public async Task InsertNotification_WithMultipleUsers_ShouldCallSignalRForEachUser()
    {
        // Arrange
        const string type = NotificationType.Error;
        const int requestId = 123;
        const string content = "Test content";

        var expectedNotifications = new List<NotificationModel>
        {
            new() { Id = 1, Content = content, Type = type }
        };
        var expectedUsers = new List<string> { "user1", "user2", "user3" };

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((expectedNotifications, expectedUsers));

        hubClients.User(Arg.Any<string>()).Returns(clientEvent);

        // Act
        await service.InsertNotification(type, requestId, content);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>());
        hubClients.Received(3).User(Arg.Any<string>());
    }

    [Fact]
    public void ExecuteBackground_WithAsyncAction_ShouldExecuteCorrectly()
    {
        // Arrange
        var executionCount = 0;

        // Act
        service.ExecuteBackground(Action);

        // Assert
        Task.Delay(200)?.Wait();
        Assert.Equal(1, executionCount);
        return;

        async Task Action()
        {
            await Task.Delay(50);
            executionCount++;
        }
    }

    [Fact]
    public async Task InsertNotification_WithLargeContent_ShouldHandleCorrectly()
    {
        // Arrange
        const string type = NotificationType.Error;
        const int requestId = 123;
        var largeContent = new string('A', 10000); // 10KB content

        var expectedNotifications = new List<NotificationModel>
        {
            new() { Id = 1, Content = largeContent, Type = type }
        };
        var expectedUsers = new List<string> { "user1" };

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((expectedNotifications, expectedUsers));

        // Act
        await service.InsertNotification(type, requestId, largeContent);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(
            Arg.Is<UpdateNotificationCriteria>(criteria => criteria.Content == largeContent));
    }

    [Fact]
    public async Task InsertNotification_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        const string type = NotificationType.Notification;
        const int requestId = 123;
        const string content = "Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        var expectedNotifications = new List<NotificationModel>
        {
            new() { Id = 1, Content = content, Type = type }
        };
        var expectedUsers = new List<string> { "user1" };

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((expectedNotifications, expectedUsers));

        // Act
        await service.InsertNotification(type, requestId, content);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(
            Arg.Is<UpdateNotificationCriteria>(criteria => criteria.Content == content));
    }

    [Fact]
    public async Task InsertNotification_WithUnicodeContent_ShouldHandleCorrectly()
    {
        // Arrange
        const string type = NotificationType.Error;
        const int requestId = 123;
        const string content = "Unicode: 中文, 日本語, 한국어, العربية, हिन्दी, русский";

        var expectedNotifications = new List<NotificationModel>
        {
            new() { Id = 1, Content = content, Type = type }
        };
        var expectedUsers = new List<string> { "user1" };

        repository.FetchMultipleAsync<NotificationModel, string>(Arg.Any<UpdateNotificationCriteria>())
            .Returns((expectedNotifications, expectedUsers));

        // Act
        await service.InsertNotification(type, requestId, content);

        // Assert
        await repository.Received(1).FetchMultipleAsync<NotificationModel, string>(
            Arg.Is<UpdateNotificationCriteria>(criteria => criteria.Content == content));
    }
}