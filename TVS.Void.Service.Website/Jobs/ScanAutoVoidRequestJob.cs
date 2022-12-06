using System;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Website.Jobs
{
    public class ScanAutoVoidRequestJob : IJob
    {
        private readonly LoggingService loggingService;
        private readonly IMaintenanceService maintenanceService;
        private readonly ScanAutoVoidRequestService scanningService;

        public ScanAutoVoidRequestJob(IMaintenanceService maintenanceService, LoggingService loggingService, ScanAutoVoidRequestService scanningService)
        {
            this.maintenanceService = maintenanceService;
            this.loggingService = loggingService;
            this.scanningService = scanningService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!AppConfigService.AppConfig.IsScanAutoVoidRequestJobExecuting)
                {
                    AppConfigService.AppConfig.IsScanAutoVoidRequestJobExecuting = true;

                    if (!await maintenanceService.IsUnderMaintenanceAsync())
                    {
                        await ExecuteSiteDbSequential();
                    }

                    AppConfigService.AppConfig.IsScanAutoVoidRequestJobExecuting = false;
                }
            }
            catch (Exception ex)
            {
                AppConfigService.AppConfig.IsScanAutoVoidRequestJobExecuting = false;
                await loggingService.ErrorAsync($"{nameof(ScanAutoVoidRequestJob)} execute failed!", ex);
            }
        }

        private async Task ExecuteSiteDbSequential()
        {
            foreach (var server in SiteDbGroupConstant.ServerList)
            {
                await scanningService.Execute(server.Id);
            }
        }
    }
}