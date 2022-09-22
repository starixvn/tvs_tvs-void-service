using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ONEbook.UM.Services;
using Quartz;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.CheckingVoidStatus;

namespace TVS.Void.Service.Website.Jobs
{
    public class CheckingVoidStatusJob : IJob
    {
        private readonly LoggingService loggingService;
        private readonly IMaintenanceService maintenanceService;
        private readonly CheckingVoidStatusService checkingVoidStatusService;
        private readonly ILogger<CheckingVoidStatusJob> logger;

        public CheckingVoidStatusJob(IMaintenanceService maintenanceService, LoggingService loggingService, CheckingVoidStatusService checkingVoidStatusService,
            ILogger<CheckingVoidStatusJob> logger)
        {
            this.maintenanceService = maintenanceService;
            this.loggingService = loggingService;
            this.checkingVoidStatusService = checkingVoidStatusService;
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
                        await checkingVoidStatusService.Execute();
                    }

                    AppConfigService.AppConfig.IsCheckingVoidStatusJobExecuting = false;
                }
            }
            catch (Exception ex)
            {
                AppConfigService.AppConfig.IsCheckingVoidStatusJobExecuting = false;
                await loggingService.ErrorAsync("CheckingVoidStatusJob execute failed!", ex);
            }
        }
    }
}