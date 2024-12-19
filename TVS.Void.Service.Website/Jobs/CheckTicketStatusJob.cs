using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ONEbook.UM.Services;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.CheckTicketStatus;

namespace TVS.Void.Service.Website.Jobs;

public class CheckTicketStatusJob(
    IMaintenanceService maintenanceService,
    LoggingService loggingService,
    CheckTicketStatusService checkTicketStatusService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await TryDoTask();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
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
            await loggingService.ErrorAsync("CheckTicketStatusJob execute failed!", ex);
        }
    }

    private async Task DoTask()
    {
        if (await maintenanceService.IsUnderMaintenanceAsync())
        {
            return;
        }

        await checkTicketStatusService.Execute();
    }
}