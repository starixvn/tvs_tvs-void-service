using System;
using System.Collections.Generic;
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
                        await ExecuteSiteDbParallel();
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

        private async Task ExecuteSiteDbParallel()
        {
            var tasks = new List<Task>();
            foreach (var server in SiteDbGroupConstant.ServerList)
            {
                var task = scanningService.Execute(server.Id);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray());
        }
    }
}