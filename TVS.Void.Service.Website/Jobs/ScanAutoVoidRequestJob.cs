using System;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Website.Jobs;

[DisallowConcurrentExecution]
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
        var currentSiteDbGroupId = context.MergedJobDataMap.GetIntValue("siteDbGroupId");
        try
        {
            if (!await maintenanceService.IsUnderMaintenanceAsync())
            {
                await scanningService.Execute(currentSiteDbGroupId);
            }
        }
        catch (Exception ex)
        {
            await loggingService.ErrorAsync($"{nameof(ScanAutoVoidRequestJob)} execute failed!", ex);
        }
    }
}