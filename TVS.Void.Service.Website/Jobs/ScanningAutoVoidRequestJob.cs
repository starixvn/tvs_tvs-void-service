using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ONEbook.UM.Services;
using Quartz;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanningAutoVoidRequest;

namespace TVS.Void.Service.Website.Jobs
{
    public class ScanningAutoVoidRequestJob : IJob
    {
        private readonly LoggingService loggingService;
        private readonly IMaintenanceService maintenanceService;
        private readonly ScanningAutoVoidRequestService scanningService;
        private readonly ILogger<ScanningAutoVoidRequestJob> logger;

        public ScanningAutoVoidRequestJob(IMaintenanceService maintenanceService, LoggingService loggingService, ScanningAutoVoidRequestService scanningService,
            ILogger<ScanningAutoVoidRequestJob> logger
)
        {
            this.maintenanceService = maintenanceService;
            this.loggingService = loggingService;
            this.scanningService = scanningService;
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
                        foreach (var server in SiteDbGroupConstant.ServerList)
                        {
                            await scanningService.Execute(server.Id);
                        }
                    }

                    AppConfigService.AppConfig.IsScanningAutoVoidRequestJobExecuting = false;
                }
            }
            catch (Exception ex)
            {
                AppConfigService.AppConfig.IsScanningAutoVoidRequestJobExecuting = false;
                await loggingService.ErrorAsync("ScanningAutoVoidRequestJob execute failed!", ex);
            }
        }
    }
}