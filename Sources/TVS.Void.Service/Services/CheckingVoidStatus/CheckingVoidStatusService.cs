using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TVS.Void.Service.Services.CheckingVoidStatus
{
    public interface CheckingVoidStatusService
    {
        Task Execute();
    }

    public class CheckingVoidStatusServiceImp : CheckingVoidStatusService
    {
        private StringBuilder logData;
        private readonly RequestDetailService requestDetailService;
        private readonly LoggingService loggingService;

        public CheckingVoidStatusServiceImp(RequestDetailService requestDetailService, LoggingService loggingService)
        {
            this.requestDetailService = requestDetailService;
            this.loggingService = loggingService;
        }

        public async Task Execute()
        {
            logData = new StringBuilder();
            Stopwatch timer = new();
            timer.Start();
            var voidServiceList = await requestDetailService.GetVoidServiceDataForCheckStatus();
            if (voidServiceList == null)
            {
                return;
            }

            try
            {
                logData.AppendLine($"{nameof(CheckingVoidStatusService)}: {JsonSerializer.Serialize(voidServiceList)}");
                foreach (var voidService in voidServiceList)
                {
                }

                timer.Stop();
                logData.AppendLine($"Server Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss}");
                logData.AppendLine($"Elapsed Time: {timer.ElapsedMilliseconds} ms");
                await loggingService.InfoAsync(logData.ToString());
            }
            catch (Exception e)
            {
                await loggingService.InfoAsync(logData.ToString());
                await loggingService.ErrorAsync($"{nameof(CheckingVoidStatusService)} Error", e);
            }
        }
    }
}