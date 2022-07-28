using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Logging;
using Microsoft.Extensions.Hosting;

namespace TVS.Void.Service.Website.Utility
{
    [ExcludeFromCodeCoverage]
    public class CustomLifetimeService : IHostedService
    {
        public CustomLifetimeService(IHostApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //// loggingService.Info("1. StartAsync has been called.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //// loggingService.Info("4. StopAsync has been called.");

            return Task.CompletedTask;
        }

        private static void OnStarted()
        {
            LogService.Info($"TVS.Void.Service - Started - {DateTime.Now}");
        }

        private static void OnStopping()
        {
            //// loggingService.Info("3. OnStopping has been called.");
        }

        private static void OnStopped()
        {
            LogService.Info($"TVS.Void.Service - STOPPED!!! - {DateTime.Now}");
        }
    }
}