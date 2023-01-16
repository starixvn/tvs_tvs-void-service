using System;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest
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
            var requestIdList = queue.RequestIds.Split(",");
            if (requestIdList.Length > 1)
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "Too many RequestIds.");
                return false;
            }

            var (request, _) = await requestDetailService.GetRequestById(int.Parse(requestIdList.First()));
            if (!request.Any())
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "RequestId invalid.");
                return false;
            }

            if (!request.First().ActionName.Equals(queue.Action, StringComparison.OrdinalIgnoreCase))
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "Request Action was edited.");
                return false;
            }

            return true;
        }
    }
}