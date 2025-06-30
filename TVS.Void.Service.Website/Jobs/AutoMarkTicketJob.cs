using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.AutoMarkTicket;

namespace TVS.Void.Service.Website.Jobs;

public class AutoMarkTicketJob(
    IMaintenanceService maintenanceService,
    LoggingService loggingService,
    AutoMarkTicketService autoMarkTicketService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await TryDoTask();
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
            await loggingService.ErrorAsync("AutoMarkTicketJob execute failed!", ex);
        }
    }

    private async Task DoTask()
    {
        if (await maintenanceService.IsUnderMaintenanceAsync() || SiteDbGroupConstant.ServerList.Count == 0)
        {
            return;
        }

        await autoMarkTicketService.Execute();
    }
}