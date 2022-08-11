using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        private readonly CheckAutoVoidRequestQueueStage checkAutoVoidRequestQueueStage;
        private readonly RequestDetailService requestDetailService;
        private readonly LoggingService loggingService;

        public CheckingVoidStatusServiceImp(CheckAutoVoidRequestQueueStage checkAutoVoidRequestQueueStage, RequestDetailService requestDetailService, LoggingService loggingService)
        {
            this.checkAutoVoidRequestQueueStage = checkAutoVoidRequestQueueStage;
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

                var firstVoidService = voidServiceList.First();
                var checkAutoVoidRequestQueue = await checkAutoVoidRequestQueueStage.Execute(firstVoidService.IsAutoVoid, firstVoidService.BroadcastSessionId);
                logData.AppendLine(checkAutoVoidRequestQueue.LogData);
                if (!checkAutoVoidRequestQueue.IsContinueFlow)
                {
                    return;
                }

                var afterVoidData = PrepareAfterVoidData(voidServiceList);
                var insertAfterVoidResult = await requestDetailService.InsertAfterVoidServiceData(firstVoidService.IsAutoVoid, afterVoidData);

                logData.AppendLine($"insertAfterVoidResult: {JsonSerializer.Serialize(insertAfterVoidResult)}");
                logData.AppendLine($"Server Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss}");
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

        private IEnumerable<AfterVoidServiceModel> PrepareAfterVoidData(IEnumerable<VoidServiceModel> voidServiceList)
        {
            var afterVoidData = voidServiceList.Select(x => new AfterVoidServiceModel
            { RequestId = x.RequestId, VoidingHistoryId = x.VoidingHistoryId }).Distinct();

            return afterVoidData;
        }
    }
}