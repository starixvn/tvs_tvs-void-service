using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest
{
    public interface ScanAutoVoidRequestService
    {
        Task Execute(int siteDbGroupId);
    }

    public class ScanAutoVoidRequestServiceImp : ScanAutoVoidRequestService
    {
        private readonly CheckVoidInstantRequestQueueValidStage checkRequestQueueValidStage;
        private readonly GetTicketsStage getTicketsStage;
        private readonly CheckTicketAllowToVoidStage checkTicketAllowToVoidStage;
        private readonly ReCalculateTicketsStage reCalculateTicketsStage;
        private readonly InsertPreVoidingDataStage insertPreVoidingDataStage;

        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly LoggingService loggingService;

        public ScanAutoVoidRequestServiceImp(CheckVoidInstantRequestQueueValidStage checkRequestQueueValidStage, GetTicketsStage getTicketsStage,
            CheckTicketAllowToVoidStage checkTicketAllowToVoidStage, ReCalculateTicketsStage reCalculateTicketsStage,
            InsertPreVoidingDataStage insertPreVoidingDataStage, AutoVoidRequestQueueService autoVoidRequestQueueService, LoggingService loggingService)
        {
            this.checkRequestQueueValidStage = checkRequestQueueValidStage;
            this.getTicketsStage = getTicketsStage;
            this.checkTicketAllowToVoidStage = checkTicketAllowToVoidStage;
            this.reCalculateTicketsStage = reCalculateTicketsStage;
            this.insertPreVoidingDataStage = insertPreVoidingDataStage;

            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.loggingService = loggingService;
        }

        public async Task Execute(int siteDbGroupId)
        {
            var queue = await autoVoidRequestQueueService.GetQueue(siteDbGroupId);
            if (queue == null)
            {
                return;
            }

            await ExecuteWhenHaveData(siteDbGroupId, queue);
        }

        private async Task ExecuteWhenHaveData(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
            var logData = new List<string>();
            try
            {
                logData.Add($"Scan[{siteDbGroupId}]: {JsonSerializer.Serialize(queue)}");
                if (IsVoidInstantQueue(queue.Action))
                {
                    await ExecuteForVoidInstantGroup(siteDbGroupId, queue, logData);
                }
                else
                {
                    await ExecuteForAutoVoidWinningGroup(siteDbGroupId, queue, logData);
                }
            }
            catch (Exception e)
            {
                await loggingService.InfoAppendNewLineAsync(logData);
                await loggingService.ErrorAsync($"{nameof(ScanAutoVoidRequestService)} Error", e);
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Failed, null);
            }
        }

        private async Task ExecuteForVoidInstantGroup(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData)
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

            var queueErrorMessage = await insertPreVoidingDataStage.Execute(siteDbGroupId, queue,
                getTicketsResult.OutputData.SingleTickets, getTicketsResult.OutputData.ParlayTickets, logData);
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
            await WriteLog(logData);
        }

        private async Task ExecuteForAutoVoidWinningGroup(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData)
        {
            var getTicketsResult = await getTicketsStage.Execute(siteDbGroupId, queue, logData);
            if (!getTicketsResult.IsContinueFlow)
            {
                return;
            }

            var checkTicketResult = await checkTicketAllowToVoidStage.Execute(siteDbGroupId,
                getTicketsResult.OutputData.SingleTickets, getTicketsResult.OutputData.ParlayTickets, logData);

            await reCalculateTicketsStage.Execute(siteDbGroupId,
                checkTicketResult.SingleTicketsResettle,
                checkTicketResult.ParlayTicketsResettle, logData);

            var queueErrorMessage = await insertPreVoidingDataStage.Execute(siteDbGroupId, queue,
                checkTicketResult.ValidVoidSingleTickets,
                checkTicketResult.ValidVoidParlayTickets, logData);

            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, queueErrorMessage);
            await WriteLog(logData);
        }

        private static bool IsVoidInstantQueue(string action)
        {
            return action.Equals(AutoVoidConstant.QueueActionVoidInstant, StringComparison.OrdinalIgnoreCase)
                   || action.Equals(AutoVoidConstant.QueueActionVoidInstant5m, StringComparison.OrdinalIgnoreCase)
                   || action.Equals(AutoVoidConstant.QueueActionVoidInstant10m, StringComparison.OrdinalIgnoreCase)
                   || action.Equals(AutoVoidConstant.QueueActionVoidInstantNoPolicy, StringComparison.OrdinalIgnoreCase);
        }

        private async Task WriteLog(IList<string> logData)
        {
            logData.Add($"ServerTime: {DateTime.Now:MM/dd/yyyy HH:mm:ss.fff}");
            await loggingService.InfoAppendNewLineAsync(logData);
        }
    }
}