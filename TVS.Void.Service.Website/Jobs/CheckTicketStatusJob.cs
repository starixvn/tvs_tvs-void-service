using System;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TVS.Void.Service.Services;
using TVS.Void.Service.Services.CheckTicketStatus;

namespace TVS.Void.Service.Website.Jobs;

public class CheckTicketStatusJob : IJob
{
    private readonly LoggingService loggingService;
    private readonly IMaintenanceService maintenanceService;
    private readonly CheckTicketStatusService checkTicketStatusService;

    public CheckTicketStatusJob(IMaintenanceService maintenanceService, LoggingService loggingService, CheckTicketStatusService checkTicketStatusService)
    {
        this.maintenanceService = maintenanceService;
        this.loggingService = loggingService;
        this.checkTicketStatusService = checkTicketStatusService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            if (!AppConfigService.AppConfig.IsCheckTicketStatusJobExecuting)
            {
                AppConfigService.AppConfig.IsCheckTicketStatusJobExecuting = true;

                if (!await maintenanceService.IsUnderMaintenanceAsync())
                {
                    await checkTicketStatusService.Execute();
                }

                AppConfigService.AppConfig.IsCheckTicketStatusJobExecuting = false;
            }
        }
        catch (Exception ex)
        {
            AppConfigService.AppConfig.IsCheckTicketStatusJobExecuting = false;
            await loggingService.ErrorAsync("CheckTicketStatusJob execute failed!", ex);
        }
    }
}