using System;
using System.Threading.Tasks;
using ONEbook.UM.Services;
using Quartz;
using TicketVoidingSystem.Services;
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
            if (!AppSettingService.AppSetting.IsCheckTicketStatusJobExecuting)
            {
                AppSettingService.AppSetting.IsCheckTicketStatusJobExecuting = true;

                if (!await maintenanceService.IsUnderMaintenanceAsync())
                {
                    await checkTicketStatusService.Execute();
                }

                AppSettingService.AppSetting.IsCheckTicketStatusJobExecuting = false;
            }
        }
        catch (Exception ex)
        {
            AppSettingService.AppSetting.IsCheckTicketStatusJobExecuting = false;
            await loggingService.ErrorAsync("CheckTicketStatusJob execute failed!", ex);
        }
    }
}