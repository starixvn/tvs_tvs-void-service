using System.Text.Json;
using Fanex.Data.Repository;
using NSubstitute;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Criteria.JackTimeHybridParlay;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Models.JackTimeHybridParlay;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.Services.JackTimeHybridParlay;

namespace TVS.Void.Service.Tests.Services.JackTimeHybridParlay;

public class HybridParlayTicketServiceTests
{
    private readonly JackTimeHybridParlayService jackTimeService = Substitute.For<JackTimeHybridParlayService>();
    private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();
    private readonly LoggingService loggingService = Substitute.For<LoggingService>();
    private readonly LegOddsService legOddsService = Substitute.For<LegOddsService>();

    [Fact]
    public async Task Execute_RecoverMainType_ShouldReturnTrue()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const int fakeUserId = 1;
        const string fakeTransIds = "123,456";
        const int recoverMainTypeId = VoidingTypeConstant.RecoverHybridParlay;
        var service = new HybridParlayTicketServiceImp(null, null, null, null);

        // Act
        var actual = await service.Execute(fakeTransIds, fakeSiteDbGroupId, recoverMainTypeId, fakeUserId);

        // Assert
        Assert.True(actual);
        await jackTimeService.DidNotReceive().Execute(Arg.Any<HybridParlayApiModel>(), Arg.Any<HybridParlayType>());
        await legOddsService.DidNotReceive().Execute(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Execute_EmptyTransIds_ShouldReturnTrue()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const int fakeUserId = 1;
        const string emptyTransIds = "";
        const int voidMainTypeId = VoidingTypeConstant.HybridParlay;
        var service = new HybridParlayTicketServiceImp(jackTimeService, repository, loggingService, legOddsService);

        // Act
        var actual = await service.Execute(emptyTransIds, fakeSiteDbGroupId, voidMainTypeId, fakeUserId);

        // Assert
        Assert.True(actual);
        await jackTimeService.DidNotReceive().Execute(Arg.Any<HybridParlayApiModel>(), Arg.Any<HybridParlayType>());
    }

    [Fact]
    public async Task Execute_NullTransIds_ShouldReturnTrue()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const int fakeUserId = 1;
        const string? nullTransIds = null;
        const int voidMainTypeId = VoidingTypeConstant.HybridParlay;
        var service = new HybridParlayTicketServiceImp(jackTimeService, repository, loggingService, legOddsService);

        // Act
        var actual = await service.Execute(nullTransIds, fakeSiteDbGroupId, voidMainTypeId, fakeUserId);

        // Assert
        Assert.True(actual);
        await jackTimeService.DidNotReceive().Execute(Arg.Any<HybridParlayApiModel>(), Arg.Any<HybridParlayType>());
    }

    [Fact]
    public async Task Execute_VoidMainTicket_ShouldReturnTrueWithoutCallingLegOdds()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const string fakeTransIds = "123,456";
        const int voidMainTypeId = VoidingTypeConstant.HybridParlay;
        var fakeTickets = new List<HybridParlayTicketModel>
        {
            new()
            {
                MainTransId = "123",
                CustId = 100,
                SubMatchId = 1,
                Transdesc = JsonSerializer.Serialize(new { id = "trans-id-123" })
            }
        };

        jackTimeService.Execute(Arg.Any<HybridParlayApiModel>(), Arg.Any<HybridParlayType>())
            .Returns(true);
        repository.FetchAsync<HybridParlayTicketModel>(Arg.Any<HybridParlayTicketCriteria>())
            .ReturnsForAnyArgs(fakeTickets);
        const int fakeUserId = 1;
        var service = new HybridParlayTicketServiceImp(jackTimeService, repository, loggingService, legOddsService);

        // Act
        var actual = await service.Execute(fakeTransIds, fakeSiteDbGroupId, voidMainTypeId, fakeUserId);

        // Assert
        Assert.True(actual);
        await jackTimeService.Received(1).Execute(Arg.Any<HybridParlayApiModel>(), HybridParlayType.VoidMain);
        await legOddsService.DidNotReceive().Execute(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Execute_VoidSubTicket_ShouldCallLegOddsWhenJackTimeSuccess()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const string fakeTransIds = "123,456";
        const int voidSubTypeId = VoidingTypeConstant.SubHybridParlay;
        var fakeTickets = new List<HybridParlayTicketModel>
        {
            new()
            {
                MainTransId = "123",
                CustId = 100,
                SubMatchId = 1,
                SubBetTeam = "1",
                SubBetTypeId = 2,
                SubHdp1 = 1,
                Transdesc = JsonSerializer.Serialize(new { id = "trans-id-123" })
            }
        };

        // Setup ServerList để GetTicketsDetail có thể trả về data
        SiteDbGroupConstant.SetServerList([new SiteDbGroupModel { Id = 1, ServerName = "DBSITE01" }]);
        jackTimeService.Execute(Arg.Any<HybridParlayApiModel>(), Arg.Any<HybridParlayType>())
            .Returns(true);
        repository.FetchAsync<HybridParlayTicketModel>(Arg.Any<HybridParlayTicketCriteria>())
            .ReturnsForAnyArgs(fakeTickets);
        const int fakeUserId = 1;
        legOddsService.Execute(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(true);
        var service = new HybridParlayTicketServiceImp(jackTimeService, repository, loggingService, legOddsService);

        // Act
        var actual = await service.Execute(fakeTransIds, fakeSiteDbGroupId, voidSubTypeId, fakeUserId);

        // Assert
        Assert.True(actual);
        await jackTimeService.Received(1).Execute(Arg.Any<HybridParlayApiModel>(), HybridParlayType.VoidSub);
        await legOddsService.Received(1).Execute(1, "123", Arg.Any<DateTime>(), fakeSiteDbGroupId, fakeUserId);
    }

    [Fact]
    public async Task Execute_VoidSubTicket_JackTimeFails_ShouldReturnFalse()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const string fakeTransIds = "123,456";
        const int voidSubTypeId = VoidingTypeConstant.SubHybridParlay;
        var fakeTickets = new List<HybridParlayTicketModel>
        {
            new()
            {
                MainTransId = "123",
                CustId = 100,
                SubMatchId = 1,
                Transdesc = JsonSerializer.Serialize(new { id = "trans-id-123" })
            }
        };

        jackTimeService.Execute(Arg.Any<HybridParlayApiModel>(), Arg.Any<HybridParlayType>())
            .Returns(false);
        var repository = Substitute.For<IDynamicRepository>();
        repository.FetchAsync<HybridParlayTicketModel>(Arg.Any<HybridParlayTicketCriteria>())
            .ReturnsForAnyArgs(fakeTickets);
        const int fakeUserId = 1;
        var service = new HybridParlayTicketServiceImp(jackTimeService, repository, loggingService, legOddsService);

        // Act
        var actual = await service.Execute(fakeTransIds, fakeSiteDbGroupId, voidSubTypeId, fakeUserId);

        // Assert
        Assert.False(actual);
        await jackTimeService.Received(1).Execute(Arg.Any<HybridParlayApiModel>(), HybridParlayType.VoidSub);
        await legOddsService.DidNotReceive().Execute(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Execute_ExceptionOccurs_ShouldLogErrorAndReturnBasedOnType()
    {
        // Arrange
        const int fakeSiteDbGroupId = 1;
        const string fakeTransIds = "123,456";
        const int voidMainTypeId = VoidingTypeConstant.HybridParlay;
        const int fakeUserId = 1;
        var exception = new Exception("Test exception");
        // Setup ServerList để GetTicketsDetail có thể throw exception
        SiteDbGroupConstant.SetServerList([new SiteDbGroupModel { Id = 1, ServerName = "DBSITE01" }]);
        repository.FetchAsync<HybridParlayTicketModel>(Arg.Any<HybridParlayTicketCriteria>())
            .Returns(Task.FromException<IEnumerable<HybridParlayTicketModel>>(exception));
        var service = new HybridParlayTicketServiceImp(jackTimeService, repository, loggingService, legOddsService);

        // Act
        var actual = await service.Execute(fakeTransIds, fakeSiteDbGroupId, voidMainTypeId, fakeUserId);

        // Assert
        Assert.True(actual); // IsNotUpdateLegOdds returns true for VoidMain
        await loggingService.Received(1).ErrorAsync($"{nameof(HybridParlayTicketService)}.{nameof(HybridParlayTicketService.Execute)} Error", Arg.Any<Exception>());
    }

    [Fact]
    public async Task GetTicketsDetail_ShouldCallRepositoryWithCorrectCriteria()
    {
        // Arrange
        const string fakeTransIds = "123,456";
        const int fakeTicketType = HybridParlayTicketType.Main;
        var fakeTickets = new List<HybridParlayTicketModel>
        {
            new() { MainTransId = "123", CustId = 100 }
        };

        // Setup ServerList với 1 server để GetTicketsDetail gọi FetchAsync 1 lần
        SiteDbGroupConstant.SetServerList([new SiteDbGroupModel { Id = 1, ServerName = "DBSITE01" }]);
        repository.FetchAsync<HybridParlayTicketModel>(Arg.Any<HybridParlayTicketCriteria>())
            .ReturnsForAnyArgs(fakeTickets);
        var service = new HybridParlayTicketServiceImp(jackTimeService, repository, loggingService, legOddsService);

        // Act
        var actual = await service.GetTicketsDetail(fakeTransIds, fakeTicketType);

        // Assert
        await repository.Received(1).FetchAsync<HybridParlayTicketModel>(
            Arg.Is<HybridParlayTicketCriteria>(x =>
                x.TransIds == fakeTransIds && x.TicketType == fakeTicketType));
        Assert.NotNull(actual);
        Assert.Single(actual);
    }
}