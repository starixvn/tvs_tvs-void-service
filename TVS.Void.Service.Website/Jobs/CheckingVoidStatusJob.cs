using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ONEbook.UM.Services;
using Quartz;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Website.Jobs
{
    public class CheckingVoidStatusJob : IJob
    {
        private readonly LoggingService loggingService;
        private readonly IMaintenanceService maintenanceService;
        private readonly ILogger<CheckingVoidStatusJob> logger;

        public CheckingVoidStatusJob(IMaintenanceService maintenanceService, LoggingService loggingService, ILogger<CheckingVoidStatusJob> logger)
        {
            this.maintenanceService = maintenanceService;
            this.loggingService = loggingService;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!AppConfigService.AppConfig.IsCheckingVoidStatusJobExecuting)
                {
                    AppConfigService.AppConfig.IsCheckingVoidStatusJobExecuting = true;

                    if (!await maintenanceService.IsUnderMaintenanceAsync())
                    {
                        await Task.Delay(4000);
                        // logger.LogInformation("CheckingVoidStatusJob running: " + DateTime.Now);
                    }

                    AppConfigService.AppConfig.IsCheckingVoidStatusJobExecuting = false;
                }
            }
            catch (Exception ex)
            {
                AppConfigService.AppConfig.IsCheckingVoidStatusJobExecuting = false;
                loggingService.Error("CheckingVoidStatusJob execute failed!", ex);
            }
        }
    }
}