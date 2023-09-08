using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.AddRequest;

namespace TVS.Void.Service.Tests.Services.AddRequest;

public class CheckDuplicatedTicketsStageTests
{
    private readonly IDynamicRepository repository;
    private readonly CheckDuplicatedTicketsStageImp service;

    public CheckDuplicatedTicketsStageTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        service = new CheckDuplicatedTicketsStageImp(repository);
    }

    [Fact]
    public async Task Execute_ValidData_ReturnExpected()
    {
        // Arrange
        var results = new List<CheckDuplicatedTicketsResult> { new() };
        repository.FetchMultipleAsync<object, object, CheckDuplicatedTicketsResult>(
                Arg.Any<CheckDuplicateTicketsCriteria>())
            .ReturnsForAnyArgs((null, null, results));
        // Act
        var actual = await service.Execute("123");

        // Assert
        await repository.Received(1).FetchMultipleAsync<object, object, CheckDuplicatedTicketsResult>(
            Arg.Any<CheckDuplicateTicketsCriteria>());
        AssertExtension.Equal(results, actual);
    }
}