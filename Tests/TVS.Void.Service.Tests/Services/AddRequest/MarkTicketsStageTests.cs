using Fanex.Data.Repository;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Services.AddRequest;

namespace TVS.Void.Service.Tests.Services.AddRequest;

public class MarkTicketsStageTests : IClassFixture<AppSettingFixture>
{
    private readonly LoggingService loggingService;
    private readonly IDynamicRepository repository;
    private readonly MarkTicketsStageImp service;

    public MarkTicketsStageTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        loggingService = Substitute.For<LoggingService>();
        service = new MarkTicketsStageImp(repository, loggingService);
    }

    [Fact]
    public async Task Execute_ValidData_CalledExpected()
    {
        // Arrange
        const string markTransIds = "123";
        var logData = new List<string> { "123" };

        repository.FetchAsync<Result>(Arg.Any<MarkTicketsCriteria>()).ReturnsForAnyArgs([]);

        // Act
        await service.Execute(markTransIds, logData);

        // Assert
        await repository.Received().FetchAsync<Result>(Arg.Any<MarkTicketsCriteria>());
    }

    [Fact]
    public async Task Execute_HasException_ReturnFalse()
    {
        // Arrange
        const string markTransIds = "123";
        var logData = new List<string> { "123" };

        repository.FetchAsync<Result>(Arg.Any<MarkTicketsCriteria>()).Throws(new Exception());

        // Act
        var actual = await service.Execute(markTransIds, logData);

        // Assert
        await loggingService.Received(1).ErrorAsync(Arg.Any<Exception>());
        Assert.False(actual);
    }
}