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
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.CheckingVoidStatus;
using TVS.Void.Service.Services.ScanningAutoVoidRequest;
using TVS.Void.Service.Services.Void;
using TVS.Void.Service.Website.Jobs;
using TVS.Void.Service.Website.Jobs.Listener;

namespace TVS.Void.Service.Website.Utility
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtension
    {
        public static void LoadConfig(this IServiceCollection services)
        {
            AppConfigService.AppConfig = new ConfigSettingModel();
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

        public static void RegisterSentry(this IServiceCollection services, IConfiguration configuration)
        {
            LogManager
                .SetDefaultLogCategory(configuration.GetSection("CategorySentryLog").Value)
                .Use(new List<ILogEngine>
                {
                    new SentryLogging(new SentryEngineOptions
                    {
                        Dsn = new Dsn(configuration.GetSection("SentryClientKey").Value)
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
            services.AddSingleton<IUMCacheService, UMCacheService>();
            services.AddSingleton<IUMRestClient, UMRestClient>();
            services.AddSingleton<IPHelper, IPHelperImpl>();
            services.AddSingleton<IHttpContextHelper, CustomHttpContextHelper>();
            services.AddSingleton<IMaintenanceService, MaintenanceService>();
        }

        public static void RegisterCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDynamicRepository, DynamicRepository>();
        }

        public static void RegisterBusinessService(this IServiceCollection services)
        {
            services.AddSingleton<LoggingService, LoggingServiceImp>();
            services.AddSingleton<AutoVoidRequestQueueService, AutoVoidRequestQueueServiceImp>();
            services.AddSingleton<RequestDetailService, RequestDetailServiceImp>();
            services.AddSingleton<TicketVoidingQueueService, TicketVoidingQueueServiceImp>();

            services.AddSingleton<ScanningAutoVoidRequestService, ScanningAutoVoidRequestServiceImp>();
            services.AddSingleton<GetTicketsStage, GetTicketsStageImp>();
            services.AddSingleton<CheckStatusTicketsStage, CheckStatusTicketsStageImp>();
            services.AddSingleton<ProcessMixParlayStage, ProcessMixParlayStageImp>();
            services.AddSingleton<ReCalculateTicketsStage, ReCalculateTicketsStageImp>();
            services.AddSingleton<InsertPreVoidingDataStage, InsertPreVoidingDataStageImp>();

            services.AddSingleton<VoidService, VoidServiceImp>();

            services.AddSingleton<CheckingVoidStatusService, CheckingVoidStatusServiceImp>();
            services.AddSingleton<CheckAutoVoidRequestQueueStage, CheckAutoVoidRequestQueueStageImp>();
        }

        public static void RegisterQuartzScheduler(this IServiceCollection services)
        {
            const int maxConcurrencyThread = 10;
            services.AddTransient<ScanningAutoVoidRequestJob>();
            services.AddTransient<VoidJob>();
            services.AddTransient<CheckingVoidStatusJob>();
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

                q.ScheduleJob<ScanningAutoVoidRequestJob>(trigger => trigger
                    .WithIdentity("ScanningAutoVoidRequestJob Trigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever())
               );

                q.ScheduleJob<VoidJob>(trigger => trigger
                    .WithIdentity("VoidJob Trigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever())
                );

                q.ScheduleJob<CheckingVoidStatusJob>(trigger => trigger
                    .WithIdentity("CheckingVoidStatusJob Trigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever())
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
}