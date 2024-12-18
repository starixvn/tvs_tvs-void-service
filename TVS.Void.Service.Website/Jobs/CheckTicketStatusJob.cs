using System;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.CheckTicketStatus;

namespace TVS.Void.Service.Website.Jobs;

[DisallowConcurrentExecution]
public class CheckTicketStatusJob(
    IMaintenanceService maintenanceService,
    LoggingService loggingService,
    CheckTicketStatusService checkTicketStatusService)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            if (!await maintenanceService.IsUnderMaintenanceAsync())
            {
                await checkTicketStatusService.Execute();
            }
        }
        catch (Exception ex)
        {
            await loggingService.ErrorAsync("CheckTicketStatusJob execute failed!", ex);
        }
    }
}