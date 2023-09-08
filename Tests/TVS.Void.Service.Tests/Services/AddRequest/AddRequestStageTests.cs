using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.AddRequest;

namespace TVS.Void.Service.Tests.Services.AddRequest;

public class AddRequestStageTests
{
    private readonly IDynamicRepository repository;
    private readonly AddRequestStageImp service;

    public AddRequestStageTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        service = new AddRequestStageImp(repository);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(29)]
    public async Task Execute_HasException_CalledExpected(int matchId)
    {
        // Arrange
        var param = new CheckDuplicatedTicketsResult { MatchId = matchId };
        var logData = new List<string> { "1" };
        var logDataExpected = new List<string> { "1", "Added RequestId: 0" };

        // Act
        var actual = await service.Execute(param, logData);

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Any<AddRequestCommand>());
        AssertExtension.Equal(logDataExpected, logData);
        Assert.Equal(0, actual);
    }
}