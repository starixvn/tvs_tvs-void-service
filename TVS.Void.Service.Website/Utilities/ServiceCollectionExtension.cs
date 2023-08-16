using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;
using ONEbook.UM.Transport;
using Quartz;
using Sentry;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.AddRequest;
using TVS.Void.Service.Services.CheckTicketStatus;
using TVS.Void.Service.Services.ScanAutoVoidRequest;
using TVS.Void.Service.Services.Void;
using TVS.Void.Service.Website.Jobs;
using TVS.Void.Service.Website.Jobs.Listener;
using TVS.Void.Service.Website.Utilities.CustomServices;

namespace TVS.Void.Service.Website.Utilities;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtension
{
    public static void LoadConfig(this IServiceCollection services, IConfiguration configuration)
    {
        AppSettingService.InitSetting(configuration);
    }

    public static void RegisterFanexData(this IServiceCollection services, IConfiguration configuration)
    {
        var connections = configuration.GetSection("ConnectionStrings")
            .GetChildren()
            .AsEnumerable()
            .ToDictionary(
                x => x.Key,
                y => new ConnectionConfiguration(y.Key, y.Value)
            );

        DbSettingProviderManager
            .StartNewSession()
            .Use(connections)
            .WithSqlServer()
            .UseDefaultDbSettingProvider(configuration.GetSection("StoredProcedurePath").Value)
            .Run();
    }

    public static void RegisterSentry(this IServiceCollection services)
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

    public static void RegisterFanexExceptionHandler(this IServiceCollection services)
    {
        var configuration = new LoggingConfiguration()
            .GetHttpHeaderInfo()
            .GetHttpSessionInfo()
            .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);
        ExceptionHandler.UseConfig(configuration);
    }

    public static void RegisterONEbookUM(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new UMClientConfiguration().Initialize(configuration));
        services.AddSingleton<IUMCacheService, MemoryUMCacheService>();
        services.AddSingleton<IUMRestClient, UMRestClient>();
        services.AddSingleton<IPHelper, IPHelperImpl>();
        services.AddSingleton<IHttpContextHelper, CustomHttpContextHelper>();
        services.AddSingleton<IMaintenanceService, MaintenanceService>();
    }

    public static void RegisterSignalR(this IServiceCollection services)
    {
        services.AddSignalR().AddStackExchangeRedis(options =>
        {
            options.Configuration.EndPoints.Add(AppSettingService.AppSetting.RedisConfig.EndPoint);
            options.Configuration.Password = AppSettingService.AppSetting.RedisConfig.Password;
            options.Configuration.DefaultDatabase = AppSettingService.AppSetting.RedisConfig.DefaultDatabase;
            options.Configuration.ChannelPrefix = AppSettingService.AppSetting.RedisConfig.SignalRChannelPrefix;
        });
    }

    public static void RegisterBusinessService(this IServiceCollection services)
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
        services.AddSingleton<GetTicketsStage, GetTicketsStageImp>();
        services.AddSingleton<CheckTicketAllowToVoidStage, CheckTicketAllowToVoidStageImp>();
        services.AddSingleton<ReCalculateTicketsStage, ReCalculateTicketsStageImp>();
        services.AddSingleton<InsertPreVoidingDataStage, InsertPreVoidingDataStageImp>();
        services.AddSingleton<AutoVoidInstantProcessStage, AutoVoidInstantProcessStageImp>();
        services.AddSingleton<AutoVoidWinningProcessStage, AutoVoidWinningProcessStageImp>();

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
    }

    public static void RegisterQuartzScheduler(this IServiceCollection services)
    {
        const int maxConcurrencyThread = 10;
        services.AddTransient<CheckTicketStatusJob>();
        services.AddTransient<ScanAutoVoidRequestJob>();
        services.AddTransient<VoidJob>();
        services.AddTransient<AddRequestDuplicatedTicketsJob>();
        services.AddQuartz(q =>
        {
            // handy when part of cluster or you want to otherwise identify multiple schedulers
            q.SchedulerId = "Scheduler-TVS-Void-Service";
            q.UseMicrosoftDependencyInjectionJobFactory();

            // these are the defaults
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = maxConcurrencyThread;
            });

            const int schedulerInterval = 5;
            q.ScheduleJob<CheckTicketStatusJob>(trigger => trigger
                .WithIdentity($"{nameof(CheckTicketStatusJob)} Trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(schedulerInterval).RepeatForever())
            );

            q.ScheduleJob<ScanAutoVoidRequestJob>(trigger => trigger
                .WithIdentity($"{nameof(ScanAutoVoidRequestJob)} Trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever())
            );

            q.ScheduleJob<VoidJob>(trigger => trigger
                .WithIdentity($"{nameof(VoidJob)}Trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever())
            );

            q.ScheduleJob<AddRequestDuplicatedTicketsJob>(trigger => trigger
                .WithIdentity($"{nameof(AddRequestDuplicatedTicketsJob)} Trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever())
            );

            q.AddSchedulerListener<CustomSchedulerListener>();

            //// auto-interrupt long-running job
            //// q.UseJobAutoInterrupt(options =>
            //// {
            ////    // this is the default
            ////    options.DefaultMaxRunTime = TimeSpan.FromMinutes(5);
            //// });
        });

        services.AddQuartzHostedService(options =>
        {
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = true;
        });
    }
}