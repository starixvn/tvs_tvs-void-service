using System;
using System.Linq;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Website.Jobs;

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
            if (!AppSettingService.AppSetting.IsVoidJobExecuting)
            {
                AppSettingService.AppSetting.IsVoidJobExecuting = true;

                if (!await maintenanceService.IsUnderMaintenanceAsync())
                {
                    await ExecuteSiteDbParallel();
                }

                AppSettingService.AppSetting.IsVoidJobExecuting = false;
            }
        }
        catch (Exception ex)
        {
            AppSettingService.AppSetting.IsVoidJobExecuting = false;
            await loggingService.ErrorAsync("VoidJob execute failed!", ex);
        }
    }

    private async Task ExecuteSiteDbParallel()
    {
        var tasks = SiteDbGroupConstant.ServerList.Select(server => voidService.Execute(server.Id));
        await Task.WhenAll(tasks);
    }
}