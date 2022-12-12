using System.Collections.Generic;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void
{
    public interface ProcessVoidQueuesStage
    {
        Task<IEnumerable<TicketVoidingQueueModel>> Execute(int siteDbGroupId, IEnumerable<TicketVoidingQueueModel> voidingQueues);
    }

    public class ProcessVoidQueuesStageImp : ProcessVoidQueuesStage
    {
        private readonly TicketVoidingQueueService ticketVoidingQueueService;

        public ProcessVoidQueuesStageImp(TicketVoidingQueueService ticketVoidingQueueService)
        {
            this.ticketVoidingQueueService = ticketVoidingQueueService;
        }

        public async Task<IEnumerable<TicketVoidingQueueModel>> Execute(int siteDbGroupId, IEnumerable<TicketVoidingQueueModel> voidingQueues)
        {
            var processResult = new List<TicketVoidingQueueModel>();
            foreach (var queue in voidingQueues)
            {
                if (queue.StatusId is ExecuteStatus.Done or ExecuteStatus.Failed)
                {
                    processResult.Add(queue);
                    continue;
                }

                var voidResult = await ticketVoidingQueueService.ProcessVoidQueue(siteDbGroupId, queue.QueueId);
                processResult.Add(voidResult);
            }

            return processResult;
        }
    }
}