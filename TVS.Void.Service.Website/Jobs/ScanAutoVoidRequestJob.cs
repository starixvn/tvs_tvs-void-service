using System;
using System.Linq;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Website.Jobs;

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
            if (!AppSettingService.AppSetting.IsScanAutoVoidRequestJobExecuting)
            {
                AppSettingService.AppSetting.IsScanAutoVoidRequestJobExecuting = true;

                if (!await maintenanceService.IsUnderMaintenanceAsync())
                {
                    await ExecuteSiteDbParallel();
                }

                AppSettingService.AppSetting.IsScanAutoVoidRequestJobExecuting = false;
            }
        }
        catch (Exception ex)
        {
            AppSettingService.AppSetting.IsScanAutoVoidRequestJobExecuting = false;
            await loggingService.ErrorAsync($"{nameof(ScanAutoVoidRequestJob)} execute failed!", ex);
        }
    }

    private async Task ExecuteSiteDbParallel()
    {
        var tasks = SiteDbGroupConstant.ServerList.Select(server => scanningService.Execute(server.Id));
        await Task.WhenAll(tasks);
    }
}