using System;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Website.Jobs;

[DisallowConcurrentExecution]
public class VoidJob(IMaintenanceService maintenanceService, LoggingService loggingService, VoidService voidService)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var currentSiteDbGroupId = context.MergedJobDataMap.GetIntValue("siteDbGroupId");

        try
        {
            if (!await maintenanceService.IsUnderMaintenanceAsync())
            {
                await voidService.Execute(currentSiteDbGroupId);
            }
        }
        catch (Exception ex)
        {
            await loggingService.ErrorAsync($"VoidJob[{currentSiteDbGroupId}] execute failed!", ex);
        }
    }
}