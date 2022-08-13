using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<VoidJob> logger;

        public VoidJob(IMaintenanceService maintenanceService, LoggingService loggingService, VoidService voidService, ILogger<VoidJob> logger)
        {
            this.maintenanceService = maintenanceService;
            this.loggingService = loggingService;
            this.voidService = voidService;
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
                        var tasks = SiteDbGroupConstant.ServerList.Select(x => voidService.Execute(x.Id));
                        await Task.WhenAll(tasks);
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