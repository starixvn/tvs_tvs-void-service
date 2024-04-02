using System;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TicketVoidingSystem.Services;
using TVS.Void.Service.Services.AddRequest;

namespace TVS.Void.Service.Website.Jobs;

[DisallowConcurrentExecution]
public class AddRequestDuplicatedTicketsJob : IJob
{
    private readonly LoggingService loggingService;
    private readonly IMaintenanceService maintenanceService;
    private readonly AddRequestDuplicatedTicketsService addRequestDuplicatedTicketsService;

    public AddRequestDuplicatedTicketsJob(IMaintenanceService maintenanceService, LoggingService loggingService, AddRequestDuplicatedTicketsService addRequestDuplicatedTicketsService)
    {
        this.maintenanceService = maintenanceService;
        this.loggingService = loggingService;
        this.addRequestDuplicatedTicketsService = addRequestDuplicatedTicketsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            if (!await maintenanceService.IsUnderMaintenanceAsync())
            {
                await addRequestDuplicatedTicketsService.Execute();
            }
        }
        catch (Exception ex)
        {
            await loggingService.ErrorAsync($"{nameof(AddRequestDuplicatedTicketsJob)} execute failed!", ex);
        }
    }
}