using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.Void;

namespace TVS.Void.Service.Website.Jobs;

public class VoidJob(IMaintenanceService maintenanceService, LoggingService loggingService, VoidService voidService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await TryDoTask();
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task TryDoTask()
    {
        try
        {
            await DoTask();
        }
        catch (Exception ex)
        {
            await loggingService.ErrorAsync("VoidJob execute failed!", ex);
        }
    }

    private async Task DoTask()
    {
        if (await maintenanceService.IsUnderMaintenanceAsync())
        {
            return;
        }

        foreach (var siteDbGroupId in SiteDbGroupConstant.ServerList.Select(x => x.Id))
        {
            await voidService.Execute(siteDbGroupId);
        }
    }
}