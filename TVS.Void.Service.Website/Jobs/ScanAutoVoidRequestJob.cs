using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.ScanAutoVoidRequest;

namespace TVS.Void.Service.Website.Jobs;

public class ScanAutoVoidRequestJob(
    IMaintenanceService maintenanceService,
    LoggingService loggingService,
    ScanAutoVoidRequestService scanningService)
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
            await loggingService.ErrorAsync("ScanAutoVoidRequestJob execute failed!", ex);
        }
    }

    private async Task DoTask()
    {
        if (await maintenanceService.IsUnderMaintenanceAsync() || SiteDbGroupConstant.ServerList.Count == 0)
        {
            return;
        }

        foreach (var siteDbGroupId in SiteDbGroupConstant.ServerList.Select(x => x.Id))
        {
            await scanningService.Execute(siteDbGroupId);
        }
    }
}