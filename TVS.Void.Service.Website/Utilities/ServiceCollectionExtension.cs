using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using StackExchange.Redis;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.AddRequest;
using TVS.Void.Service.Services.CheckTicketStatus;
using TVS.Void.Service.Services.ScanAutoVoidRequest;
using TVS.Void.Service.Services.Void;
using TVS.Void.Service.Website.Jobs;

namespace TVS.Void.Service.Website.Utilities;

[ExcludeFromCodeCoverage(Justification = "non-logic")]
public static class ServiceCollectionExtension
{
    public static void AddTvsConfig(this IServiceCollection services, IConfiguration configuration)
    {
        AppSettingService.InitSetting(configuration);
    }

    public static void AddSentry(this IServiceCollection services)
    {
        LogManager
            .SetDefaultLogCategory(AppSettingService.AppSetting.CategorySentryLog)
            .Use(new List<ILogEngine>
            {
                new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn(AppSettingService.AppSetting.SentryClientKey)
                })
            });
    }

    public static void AddFanexExceptionHandler(this IServiceCollection services)
    {
        var configuration = new LoggingConfiguration()
            .GetHttpHeaderInfo()
            .GetHttpSessionInfo()
            .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);
        ExceptionHandler.UseConfig(configuration);
    }

    public static void AddTvsSignalR(this IServiceCollection services)
    {
        services.AddSignalR().AddStackExchangeRedis(options =>
        {
            var redisConfig = AppSettingService.AppSetting.RedisConfig;
            options.Configuration.EndPoints.Add(redisConfig.EndPoint);
            options.Configuration.Password = redisConfig.Password;
            options.Configuration.DefaultDatabase = redisConfig.DefaultDatabase;
            options.Configuration.ChannelPrefix = RedisChannel.Literal(redisConfig.SignalRChannelPrefix);
        });
    }

    public static void AddBusinessService(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicRepository, DynamicRepository>();

        services.AddSingleton<LoggingService, LoggingServiceImp>();
        services.AddSingleton<AutoVoidRequestQueueService, AutoVoidRequestQueueServiceImp>();
        services.AddSingleton<RequestDetailService, RequestDetailServiceImp>();
        services.AddSingleton<TicketVoidingQueueService, TicketVoidingQueueServiceImp>();
        services.AddSingleton<RequestTicketService, RequestTicketServiceImp>();
        services.AddSingleton<NotificationService, NotificationServiceImp>();
        services.AddSingleton<NetPositionService, NetPositionServiceImp>();

        services.AddSingleton<CheckTicketStatusService, CheckTicketStatusServiceImp>();
        services.AddSingleton<CheckTicketsForAutoVoidWinningStage, CheckTicketsForAutoVoidWinningStageImp>();
        services.AddSingleton<CheckTicketsForAutoVoidAllResultKeyedStage, CheckTicketsForAutoVoidAllResultKeyedStageImp>();
        services.AddSingleton<InsertAutoVoidQueueStage, InsertAutoVoidQueueStageImp>();

        services.AddSingleton<ScanAutoVoidRequestService, ScanAutoVoidRequestServiceImp>();
        services.AddSingleton<CheckVoidInstantRequestQueueValidStage, CheckVoidInstantRequestQueueValidStageImp>();
        services.AddSingleton<CheckVoidWinningRequestQueueValidStage, CheckVoidWinningRequestQueueValidStageImp>();
        services.AddSingleton<GetTicketsStage, GetTicketsStageImp>();
        services.AddSingleton<CheckTicketValidationStage, CheckTicketValidationStageImp>();
        services.AddSingleton<ReCalculateTicketsStage, ReCalculateTicketsStageImp>();
        services.AddSingleton<InsertPreVoidingDataStage, InsertPreVoidingDataStageImp>();
        services.AddSingleton<AutoInstantProcessStage, AutoInstantProcessStageImp>();
        services.AddSingleton<AutoVoidWinningProcessStage, AutoVoidWinningProcessStageImp>();
        services.AddSingleton<CreateVoidDataXmlStage, CreateVoidDataXmlStageImp>();

        services.AddSingleton<VoidService, VoidServiceImp>();
        services.AddSingleton<ProcessVoidQueuesStage, ProcessVoidQueuesStageImp>();
        services.AddSingleton<InsertAfterVoidDataStage, InsertAfterVoidDataStageImp>();
        services.AddSingleton<InsertAfterVoidDataPrivateService, InsertAfterVoidDataPrivateServiceImp>();

        services.AddSingleton<AddRequestDuplicatedTicketsService, AddRequestDuplicatedTicketsServiceImp>();
        services.AddSingleton<AddRequestStage, AddRequestStageImp>();
        services.AddSingleton<CheckDuplicatedTicketsStage, CheckDuplicatedTicketsStageImp>();
        services.AddSingleton<GetDuplicatedTicketsStage, GetDuplicatedTicketsStageImp>();
        services.AddSingleton<InsertAutoVoidRequestQueueStage, InsertAutoVoidRequestQueueStageImp>();
        services.AddSingleton<MarkTicketsStage, MarkTicketsStageImp>();
        services.AddSingleton<AutoVoidWinningBeforeVoidService, AutoVoidWinningBeforeVoidServiceImp>();
    }

    public static IServiceCollection AddBackgroundService(this IServiceCollection services)
    {
        services.AddSingleton<AddRequestDuplicatedTicketsJob>();
        services.AddSingleton<CheckTicketStatusJob>();
        services.AddSingleton<ScanAutoVoidRequestJob>();
        services.AddSingleton<VoidJob>();

        services.AddHostedService<AddRequestDuplicatedTicketsJob>();
        services.AddHostedService<CheckTicketStatusJob>();
        services.AddHostedService<ScanAutoVoidRequestJob>();
        services.AddHostedService<VoidJob>();

        return services;
    }
}