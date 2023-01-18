using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Website.Jobs
{
    public class VoidJob : IJob
    {
        private readonly LoggingService loggingService;
        private readonly IMaintenanceService maintenanceService;
        private readonly VoidService voidService;

        public VoidJob(IMaintenanceService maintenanceService, LoggingService loggingService, VoidService voidService)
        {
            this.maintenanceService = maintenanceService;
            this.loggingService = loggingService;
            this.voidService = voidService;
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
                        await ExecuteSiteDbParallel();
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

        private async Task ExecuteSiteDbParallel()
        {
            var tasks = new List<Task>();
            foreach (var server in SiteDbGroupConstant.ServerList)
            {
                var task = voidService.Execute(server.Id);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.ToArray());
        }

        ////private async Task ExecuteSiteDbSequential()
        ////{
        ////    foreach (var server in SiteDbGroupConstant.ServerList)
        ////    {
        ////        await voidService.Execute(server.Id);
        ////    }
        ////}
    }
}