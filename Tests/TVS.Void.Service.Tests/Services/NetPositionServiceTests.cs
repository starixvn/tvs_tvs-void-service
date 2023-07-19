using Fanex.Data.Repository;
using NSubstitute;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Tests.Services;

public class NetPositionServiceTests
{
    private readonly LoggingService loggingService;
    private readonly IDynamicRepository repository;
    private NetPositionServiceImp service;

    public NetPositionServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        loggingService = Substitute.For<LoggingService>();
        service = new NetPositionServiceImp(loggingService, repository);
    }

    [Fact]
    public async Task UpdateNetPosition_HasTransIds_HasCalledPostRequest()
    {
        // Arrange
        // Act
        var result = await service.UpdateNetPosition(0, "123", "VOID");

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Any<TicketVoidingNetPositionCriteria>());
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateNetPosition_WithEmptyTransIds_ReturnsTrue()
    {
        // Arrange
        // Act
        var result = await service.UpdateNetPosition(0, "", "VOID");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateNetPosition_WithNullTransIds_ReturnsTrue()
    {
        // Arrange
        // Act
        var result = await service.UpdateNetPosition(0, null, "VOID");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateNetPosition_HasException_ReturnsFalse()
    {
        // Arrange
        service = new NetPositionServiceImp(loggingService, null);

        // Act
        var result = await service.UpdateNetPosition(0, "1234", "VOID");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RollBackNetPosition_WithInValidTransIds_NotCalledRepositoryExecuteAsync()
    {
        // Arrange
        // Act
        await service.RollBackNetPosition(0, "");

        // Assert
        await repository.Received(0).ExecuteAsync(Arg.Any<TicketVoidingNetPositionCriteria>());
    }

    [Fact]
    public async Task RollBackNetPosition_WithValidTransIds_CalledRepositoryExecuteAsync()
    {
        // Arrange
        // Act
        await service.RollBackNetPosition(0, "1234");

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Any<TicketVoidingNetPositionCriteria>());
    }
}