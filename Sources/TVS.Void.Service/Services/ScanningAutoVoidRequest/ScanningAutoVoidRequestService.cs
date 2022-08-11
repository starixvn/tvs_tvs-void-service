using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface ScanningAutoVoidRequestService
    {
        Task Execute(int siteDbGroupId);
    }

    public class ScanningAutoVoidRequestServiceImp : ScanningAutoVoidRequestService
    {
        private StringBuilder logData;
        private readonly GetTicketsStage getTicketsStage;
        private readonly CheckStatusTicketsStage checkStatusTicketsStage;
        private readonly ProcessMixParlayStage processMixParlayStage;
        private readonly ReCalculateTicketsStage reCalculateTicketsStage;
        private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;

        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly LoggingService loggingService;

        public ScanningAutoVoidRequestServiceImp(GetTicketsStage getTicketsStage, CheckStatusTicketsStage checkStatusTicketsStage, ProcessMixParlayStage processMixParlayStage,
            ReCalculateTicketsStage reCalculateTicketsStage, InsertPreVoidingDataStage insertPreVoidingDataStage,
            AutoVoidRequestQueueService autoVoidRequestQueueService, LoggingService loggingService)
        {
            this.getTicketsStage = getTicketsStage;
            this.checkStatusTicketsStage = checkStatusTicketsStage;
            this.processMixParlayStage = processMixParlayStage;
            this.reCalculateTicketsStage = reCalculateTicketsStage;
            this.insertPreVoidingDataStage = insertPreVoidingDataStage;

            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.loggingService = loggingService;
        }

        public async Task Execute(int siteDbGroupId)
        {
            logData = new StringBuilder();
            Stopwatch timer = new();
            timer.Start();
            var queue = await autoVoidRequestQueueService.GetValidQueue(siteDbGroupId);
            if (queue == null)
            {
                return;
            }

            try
            {
                logData.AppendLine($"Scanning[{siteDbGroupId}]: {JsonSerializer.Serialize(queue)}");
                if (IsVoidInstantQueue(queue.Action))
                {
                    await ExecuteForVoidInstantGroup(siteDbGroupId, queue);
                }
                else
                {
                    await ExecuteForAutoVoidWinningGroup(siteDbGroupId, queue);
                }

                timer.Stop();
                logData.AppendLine($"Server Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss}");
                logData.AppendLine($"Elapsed Time: {timer.ElapsedMilliseconds} ms");
                await loggingService.InfoAsync(logData.ToString());
            }
            catch (Exception e)
            {
                await loggingService.InfoAsync(logData.ToString());
                await loggingService.ErrorAsync($"{nameof(ScanningAutoVoidRequestService)} Error", e);
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Failed, null, null);
            }
        }

        public async Task ExecuteForVoidInstantGroup(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
            var getTicketsResult = await getTicketsStage.Execute(siteDbGroupId, queue);

            if (!getTicketsResult.IsContinueFlow)
            {
                return;
            }

            var insertPreVoidingResult = await insertPreVoidingDataStage.Execute(siteDbGroupId, queue, getTicketsResult.OutputData.SingleTickets, getTicketsResult.OutputData.ParlayTickets);
            logData.AppendLine(getTicketsResult.LogData);
            logData.AppendLine(insertPreVoidingResult.LogData);
        }

        public async Task ExecuteForAutoVoidWinningGroup(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
            var getTicketsResult = await getTicketsStage.Execute(siteDbGroupId, queue);
            if (!getTicketsResult.IsContinueFlow)
            {
                return;
            }

            var checkStatusTicketsResult = await checkStatusTicketsStage.Execute(siteDbGroupId, getTicketsResult.OutputData.SingleTickets, getTicketsResult.OutputData.ParlayTickets);
            logData.AppendLine(checkStatusTicketsResult.LogData);

            var processMixParlayResult = await processMixParlayStage.Execute(siteDbGroupId, queue, checkStatusTicketsResult.OutputData.MixParlayRequestsNoStatusWinLossButAllChildMatchesCompleted);
            logData.AppendLine(processMixParlayResult.LogData);
            if (!processMixParlayResult.IsContinueFlow)
            {
                return;
            }

            await reCalculateTicketsStage.Execute(siteDbGroupId,
                checkStatusTicketsResult.OutputData.SingleTicketsResettle,
                checkStatusTicketsResult.OutputData.ParlayTicketsResettle);

            var insertPreVoidingDataResult = await insertPreVoidingDataStage.Execute(siteDbGroupId, queue,
                checkStatusTicketsResult.OutputData.ValidVoidSingleTickets,
                checkStatusTicketsResult.OutputData.ValidVoidParlayTickets);
            logData.AppendLine(insertPreVoidingDataResult.LogData);
        }

        private bool IsVoidInstantQueue(string action)
        {
            return action.Equals(AutoVoidConstant.QueueActionVoidInstant, StringComparison.OrdinalIgnoreCase)
                   || action.Equals(AutoVoidConstant.QueueActionVoidInstant5m, StringComparison.OrdinalIgnoreCase)
                   || action.Equals(AutoVoidConstant.QueueActionVoidInstant10m, StringComparison.OrdinalIgnoreCase)
                   || action.Equals(AutoVoidConstant.QueueActionVoidInstantNoPolicy, StringComparison.OrdinalIgnoreCase);
        }
    }
}