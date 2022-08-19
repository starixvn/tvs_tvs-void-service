using System;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface CheckRequestQueueValidStage
    {
        Task<bool> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue);
    }

    public class CheckRequestQueueValidStageImp : CheckRequestQueueValidStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly RequestDetailService requestDetailService;

        public CheckRequestQueueValidStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.requestDetailService = requestDetailService;
        }

        public async Task<bool> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
            var request = await requestDetailService.GetRequestById(queue.RequestId);
            if (request == null)
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, null, "RequestId invalid.");
                return false;
            }

            if (!request.ActionName.Equals(queue.Action, StringComparison.OrdinalIgnoreCase))
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, null, "Request Action was edited.");
                return false;
            }

            return true;
        }
    }
}