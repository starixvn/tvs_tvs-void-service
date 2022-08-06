using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ONEbook.UM.Services;
using Quartz;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Website.Jobs
{
    public class VoidJob : IJob
    {
        private readonly LoggingService loggingService;
        private readonly IMaintenanceService maintenanceService;
        private readonly ILogger<VoidJob> logger;

        public VoidJob(IMaintenanceService maintenanceService, LoggingService loggingService, ILogger<VoidJob> logger)
        {
            this.maintenanceService = maintenanceService;
            this.loggingService = loggingService;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!AppConfigService.AppConfig.IsVoidJobExecuting)
                {
                    AppConfigService.AppConfig.IsVoidJobExecuting = true;

                    if (!await maintenanceService.IsUnderMaintenanceAsync())
                    {
                        await Task.Delay(3000);
                        //logger.LogInformation("VoidJob running: " + DateTime.Now);
                    }

                    AppConfigService.AppConfig.IsVoidJobExecuting = false;
                }
            }
            catch (Exception ex)
            {
                AppConfigService.AppConfig.IsVoidJobExecuting = false;
                await loggingService.ErrorAsync("VoidJob execute failed!", ex);
            }
        }
    }
}