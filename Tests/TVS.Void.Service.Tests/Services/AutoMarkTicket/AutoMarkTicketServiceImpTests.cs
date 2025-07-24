using Fanex.Data.Repository;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TicketVoidingSystem.Models;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.AutoMarkTicket;

namespace TVS.Void.Service.Tests.Services.AutoMarkTicket;

public class AutoMarkTicketServiceImpTests
{
    private readonly IDynamicRepository repository;
    private readonly AutoMarkTicketServiceImp service;

    public AutoMarkTicketServiceImpTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        service = new AutoMarkTicketServiceImp(repository);
    }

    [Fact]
    public async Task Execute_WithNoTickets_ShouldNotProcessAnyGroups()
    {
        // Arrange
        repository.FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>())
            .Returns(new List<AutoMarkTicketModel>());

        // Act
        await service.Execute();

        // Assert
        await repository.Received(1).FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>());
        await repository.DidNotReceive().GetAsync<Result>(Arg.Any<MarkTicketsCriteria>());
        await repository.DidNotReceive().ExecuteAsync(Arg.Any<UpdateStatusPendingMarkTicketCommand>());
    }

    [Fact]
    public async Task Execute_WithSingleGroupTickets_ShouldProcessSuccessfully()
    {
        // Arrange
        var tickets = new List<AutoMarkTicketModel>
        {
            new() { TransId = "1001", SiteDbGroupId = 1 },
            new() { TransId = "1002", SiteDbGroupId = 1 },
            new() { TransId = "1003", SiteDbGroupId = 1 }
        };

        var successfulResult = new Result { ErrorCode = 0, ErrorMsg = "" };

        repository.FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>())
            .Returns(tickets);
        repository.GetAsync<Result>(Arg.Any<MarkTicketsCriteria>())
            .Returns(successfulResult);

        // Act
        await service.Execute();

        // Assert
        await repository.Received(1).FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>());
        await repository.Received(1).GetAsync<Result>(Arg.Is<MarkTicketsCriteria>(x =>
            x.SiteDbGroupId == 1 &&
            x.TransIds == "1001,1002,1003"));
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateStatusPendingMarkTicketCommand>(x =>
            x.StatusId == 2 && // ExecuteStatus.Done
            x.TransIds == "1001,1002,1003"));
    }

    [Fact]
    public async Task Execute_WithMultipleGroupTickets_ShouldProcessEachGroup()
    {
        // Arrange
        var tickets = new List<AutoMarkTicketModel>
        {
            new() { TransId = "2001", SiteDbGroupId = 2 },
            new() { TransId = "2002", SiteDbGroupId = 2 },
            new() { TransId = "3001", SiteDbGroupId = 3 }
        };

        var successfulResult = new Result { ErrorCode = 0, ErrorMsg = "" };
        var failedResult = new Result { ErrorCode = 500, ErrorMsg = "Error" };

        repository.FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>())
            .Returns(tickets);
        repository.GetAsync<Result>(Arg.Is<MarkTicketsCriteria>(x => x.SiteDbGroupId == 2))
            .Returns(failedResult);
        repository.GetAsync<Result>(Arg.Is<MarkTicketsCriteria>(x => x.SiteDbGroupId == 3))
            .Returns(successfulResult);

        // Act
        await service.Execute();

        // Assert
        await repository.Received(1).FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>());

        // Verify all groups were processed
        await repository.Received(1).GetAsync<Result>(Arg.Is<MarkTicketsCriteria>(x =>
            x.SiteDbGroupId == 2 && x.TransIds == "2001,2002"));
        await repository.Received(1).GetAsync<Result>(Arg.Is<MarkTicketsCriteria>(x =>
            x.SiteDbGroupId == 3 && x.TransIds == "3001"));

        // Verify status updates for each group
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateStatusPendingMarkTicketCommand>(x =>
            x.StatusId == 3 && x.TransIds == "2001,2002")); // Failed for group 2
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateStatusPendingMarkTicketCommand>(x =>
            x.StatusId == 2 && x.TransIds == "3001")); // Success for group 3
    }

    [Fact]
    public async Task Execute_WithFailedMarkOperation_ShouldUpdateStatusToFailed()
    {
        // Arrange
        var tickets = new List<AutoMarkTicketModel>
        {
            new() { TransId = "1001", SiteDbGroupId = 1 },
            new() { TransId = "1002", SiteDbGroupId = 1 }
        };

        var failedResult = new Result { ErrorCode = 500, ErrorMsg = "Database error" };

        repository.FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>())
            .Returns(tickets);
        repository.GetAsync<Result>(Arg.Any<MarkTicketsCriteria>())
            .Returns(failedResult);

        // Act
        await service.Execute();

        // Assert
        await repository.Received(1).GetAsync<Result>(Arg.Any<MarkTicketsCriteria>());
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateStatusPendingMarkTicketCommand>(x =>
            x.StatusId == 3 && // ExecuteStatus.Failed
            x.TransIds == "1001,1002"));
    }

    [Fact]
    public async Task Execute_WithRepositoryException_ShouldHandleGracefully()
    {
        // Arrange
        var tickets = new List<AutoMarkTicketModel>
        {
            new() { TransId = "1001", SiteDbGroupId = 1 }
        };

        repository.FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>())
            .Returns(tickets);
        repository.GetAsync<Result>(Arg.Any<MarkTicketsCriteria>())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.Execute());
        Assert.Equal("Database connection failed", exception.Message);
    }

    [Fact]
    public async Task Execute_WithEmptyTransIds_ShouldHandleCorrectly()
    {
        // Arrange
        var tickets = new List<AutoMarkTicketModel>
        {
            new() { TransId = "", SiteDbGroupId = 1 },
            new() { TransId = null, SiteDbGroupId = 1 }
        };

        var successfulResult = new Result { ErrorCode = 0, ErrorMsg = "" };

        repository.FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>())
            .Returns(tickets);
        repository.GetAsync<Result>(Arg.Any<MarkTicketsCriteria>())
            .Returns(successfulResult);

        // Act
        await service.Execute();

        // Assert
        await repository.Received(1).GetAsync<Result>(Arg.Is<MarkTicketsCriteria>(x =>
            x.TransIds == ","));
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateStatusPendingMarkTicketCommand>(x =>
            x.TransIds == ","));
    }

    [Fact]
    public async Task Execute_WithMixedSuccessAndFailureResults_ShouldUpdateStatusCorrectly()
    {
        // Arrange
        var tickets = new List<AutoMarkTicketModel>
        {
            new() { TransId = "1001", SiteDbGroupId = 1 },
            new() { TransId = "2001", SiteDbGroupId = 2 }
        };

        // Group 1 succeeds, Group 2 fails
        repository.FetchAsync<AutoMarkTicketModel>(Arg.Any<SelectAutoMarkTicketCriteria>())
            .Returns(tickets);
        repository.GetAsync<Result>(Arg.Is<MarkTicketsCriteria>(x => x.SiteDbGroupId == 1))
            .Returns(new Result { ErrorCode = 0, ErrorMsg = "" });
        repository.GetAsync<Result>(Arg.Is<MarkTicketsCriteria>(x => x.SiteDbGroupId == 2))
            .Returns(new Result { ErrorCode = 500, ErrorMsg = "Failed" });

        // Act
        await service.Execute();

        // Assert
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateStatusPendingMarkTicketCommand>(x =>
            x.StatusId == 2 && x.TransIds == "1001")); // Success
        await repository.Received(1).ExecuteAsync(Arg.Is<UpdateStatusPendingMarkTicketCommand>(x =>
            x.StatusId == 3 && x.TransIds == "2001")); // Failed
    }
}