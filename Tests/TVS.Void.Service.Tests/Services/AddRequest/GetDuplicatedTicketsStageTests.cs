using Fanex.Data.Repository;
using NSubstitute;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.AddRequest;

namespace TVS.Void.Service.Tests.Services.AddRequest;

public class GetDuplicatedTicketsStageTests
{
    private readonly IDynamicRepository repository;
    private readonly GetDuplicatedTicketsStageImp service;

    public GetDuplicatedTicketsStageTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        service = new GetDuplicatedTicketsStageImp(repository);
    }

    [Fact]
    public async Task Execute_ValidData_ReturnExpected()
    {
        // Arrange
        var expected = new DuplicatedTickets();
        repository.GetAsync<DuplicatedTickets>(Arg.Any<GetDuplicatedTicketsCriteria>()).ReturnsForAnyArgs(expected);
        // Act
        var actual = await service.Execute();

        // Assert
        await repository.Received(1).GetAsync<DuplicatedTickets>(Arg.Any<GetDuplicatedTicketsCriteria>());
        AssertExtension.Equal(expected, actual);
    }
}