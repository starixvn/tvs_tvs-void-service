using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
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
        private readonly Stopwatch timer = new();
        private readonly CheckRequestQueueValidStage checkRequestQueueValidStage;
        private readonly GetTicketsStage getTicketsStage;
        private readonly CheckStatusTicketsStage checkStatusTicketsStage;
        private readonly ProcessMixParlayStage processMixParlayStage;
        private readonly ReCalculateTicketsStage reCalculateTicketsStage;
        private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;

        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly LoggingService loggingService;

        public ScanningAutoVoidRequestServiceImp(CheckRequestQueueValidStage checkRequestQueueValidStage, GetTicketsStage getTicketsStage, CheckStatusTicketsStage checkStatusTicketsStage,
            ProcessMixParlayStage processMixParlayStage, ReCalculateTicketsStage reCalculateTicketsStage, InsertPreVoidingDataStage insertPreVoidingDataStage,
            AutoVoidRequestQueueService autoVoidRequestQueueService, LoggingService loggingService)
        {
            this.checkRequestQueueValidStage = checkRequestQueueValidStage;
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
            timer.Restart();
            var queue = await autoVoidRequestQueueService.GetQueue(siteDbGroupId, null);
            if (queue == null)
            {
                return;
            }

            await ExecuteWhenHaveData(siteDbGroupId, queue);
        }

        private async Task ExecuteWhenHaveData(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
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
                logData.AppendLine($"Server Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss.fff}");
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

        private async Task ExecuteForVoidInstantGroup(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
            var isRequestQueueValid = await checkRequestQueueValidStage.Execute(siteDbGroupId, queue);
            if (!isRequestQueueValid)
            {
                return;
            }

            var getTicketsResult = await getTicketsStage.Execute(siteDbGroupId, queue, logData);
            if (!getTicketsResult.IsContinueFlow)
            {
                return;
            }

            await insertPreVoidingDataStage.Execute(siteDbGroupId, queue,
                getTicketsResult.OutputData.SingleTickets, getTicketsResult.OutputData.ParlayTickets, logData);
        }

        private async Task ExecuteForAutoVoidWinningGroup(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
            var getTicketsResult = await getTicketsStage.Execute(siteDbGroupId, queue, null);
            if (!getTicketsResult.IsContinueFlow)
            {
                return;
            }

            var checkStatusTicketsResult = await checkStatusTicketsStage.Execute(siteDbGroupId, getTicketsResult.OutputData.SingleTickets, getTicketsResult.OutputData.ParlayTickets, logData);

            var processMixParlayResult = await processMixParlayStage.Execute(siteDbGroupId, queue, checkStatusTicketsResult.MixParlayRequestsNoStatusWinLossButAllChildMatchesCompleted, logData);
            if (!processMixParlayResult)
            {
                return;
            }

            await reCalculateTicketsStage.Execute(siteDbGroupId,
                checkStatusTicketsResult.SingleTicketsResettle,
                checkStatusTicketsResult.ParlayTicketsResettle);

            await insertPreVoidingDataStage.Execute(siteDbGroupId, queue,
                 checkStatusTicketsResult.ValidVoidSingleTickets,
                 checkStatusTicketsResult.ValidVoidParlayTickets, logData);
        }

        private static bool IsVoidInstantQueue(string action)
        {
            return action.Equals(AutoVoidConstant.QueueActionVoidInstant, StringComparison.OrdinalIgnoreCase)
                   || action.Equals(AutoVoidConstant.QueueActionVoidInstant5m, StringComparison.OrdinalIgnoreCase)
                   || action.Equals(AutoVoidConstant.QueueActionVoidInstant10m, StringComparison.OrdinalIgnoreCase)
                   || action.Equals(AutoVoidConstant.QueueActionVoidInstantNoPolicy, StringComparison.OrdinalIgnoreCase);
        }
    }
}