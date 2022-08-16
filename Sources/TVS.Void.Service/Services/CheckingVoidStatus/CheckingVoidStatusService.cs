using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckingVoidStatus
{
    public interface CheckingVoidStatusService
    {
        Task Execute();
    }

    public class CheckingVoidStatusServiceImp : CheckingVoidStatusService
    {
        private StringBuilder logData;
        private readonly Stopwatch timer = new();
        private readonly CheckAutoVoidRequestQueueStage checkAutoVoidRequestQueueStage;
        private readonly PrepareAfterVoidDataStage prepareAfterVoidDataStage;
        private readonly RequestDetailService requestDetailService;
        private readonly LoggingService loggingService;

        public CheckingVoidStatusServiceImp(CheckAutoVoidRequestQueueStage checkAutoVoidRequestQueueStage, PrepareAfterVoidDataStage prepareAfterVoidDataStage,
            RequestDetailService requestDetailService, LoggingService loggingService)
        {
            this.checkAutoVoidRequestQueueStage = checkAutoVoidRequestQueueStage;
            this.prepareAfterVoidDataStage = prepareAfterVoidDataStage;
            this.requestDetailService = requestDetailService;
            this.loggingService = loggingService;
        }

        public async Task Execute()
        {
            logData = new StringBuilder();
            timer.Start();
            var voidServiceList = await requestDetailService.GetVoidServiceDataForCheckStatus();
            if (!voidServiceList.Any())
            {
                return;
            }

            await ExecuteWhenHaveData(voidServiceList);
        }

        private async Task ExecuteWhenHaveData(IEnumerable<VoidServiceModel> voidServiceList)
        {
            try
            {
                logData.AppendLine($"{nameof(CheckingVoidStatusService)}: {JsonSerializer.Serialize(voidServiceList)}");

                var firstVoidService = voidServiceList.First();
                var checkAutoVoidRequestQueue = await checkAutoVoidRequestQueueStage.Execute(firstVoidService.IsAutoVoid, firstVoidService.BroadcastSessionId);
                logData.AppendLine(checkAutoVoidRequestQueue.LogData);
                if (!checkAutoVoidRequestQueue.IsContinueFlow)
                {
                    await loggingService.InfoAsync(logData.ToString());
                    return;
                }

                var afterVoidData = await prepareAfterVoidDataStage.Execute(voidServiceList);
                var insertAfterVoidResult = await requestDetailService.InsertAfterVoidServiceData(firstVoidService.IsAutoVoid, afterVoidData);
                logData.AppendLine($"insertAfterVoidResult: {JsonSerializer.Serialize(insertAfterVoidResult)}");

                await requestDetailService.UpdateVoidServiceByBroadcastSessionId(firstVoidService.BroadcastSessionId, ExecuteStatus.Done);

                logData.AppendLine($"Server Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss.fff}");
                timer.Stop();
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