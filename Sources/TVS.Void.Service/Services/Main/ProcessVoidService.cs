using System.Threading.Tasks;
using TVS.Void.Service.Constants;

namespace TVS.Void.Service.Services.Main
{
    public interface ProcessVoidService
    {
        Task Execute(int siteDbGroupId);
    }

    public class ProcessVoidServiceImp : ProcessVoidService
    {
        private readonly TicketVoidingQueueService ticketVoidingQueueService;
        private readonly RequestDetailService requestDetailService;

        public ProcessVoidServiceImp(TicketVoidingQueueService ticketVoidingQueueService, RequestDetailService requestDetailService)
        {
            this.ticketVoidingQueueService = ticketVoidingQueueService;
            this.requestDetailService = requestDetailService;
        }

        public async Task Execute(int siteDbGroupId)
        {
            await ticketVoidingQueueService.GetQueues(siteDbGroupId);
            // TODO: Process void
            // TODO: Update status ticket voiding queue
            await requestDetailService.UpdateVoidService(siteDbGroupId, 1, 1, ExecuteStatus.ExecuteVoidDone);
        }
    }
}