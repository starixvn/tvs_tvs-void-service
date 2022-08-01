using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ONEbook.UM.Services;
using Quartz;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Website.Jobs
{
    public class ScanningAutoVoidRequestJob : IJob
    {
        private readonly LoggingService loggingService;
        private readonly IMaintenanceService maintenanceService;
        private readonly ILogger<ScanningAutoVoidRequestJob> logger;

        public ScanningAutoVoidRequestJob(IMaintenanceService maintenanceService, LoggingService loggingService, ILogger<ScanningAutoVoidRequestJob> logger)
        {
            this.maintenanceService = maintenanceService;
            this.loggingService = loggingService;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!AppConfigService.AppConfig.IsScanningAutoVoidRequestJobExecuting)
                {
                    AppConfigService.AppConfig.IsScanningAutoVoidRequestJobExecuting = true;

                    if (!await maintenanceService.IsUnderMaintenanceAsync())
                    {
                        await Task.Delay(2000);
                        logger.LogInformation("ScanningAutoVoidRequestJob running: " + DateTime.Now);
                    }

                    AppConfigService.AppConfig.IsScanningAutoVoidRequestJobExecuting = false;
                }
            }
            catch (Exception ex)
            {
                AppConfigService.AppConfig.IsScanningAutoVoidRequestJobExecuting = false;
                loggingService.Error("ScanningAutoVoidRequestJob execute failed!", ex);
            }
        }
    }
}