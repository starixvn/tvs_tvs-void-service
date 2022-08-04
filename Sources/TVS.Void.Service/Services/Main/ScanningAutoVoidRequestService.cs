using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TVS.Void.Service.Constants;

namespace TVS.Void.Service.Services.Main
{
    public interface ScanningAutoVoidRequestService
    {
        Task Execute(int siteDbGroupId, int timeOut);
    }

    public class ScanningAutoVoidRequestServiceImp : ScanningAutoVoidRequestService
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly RequestDetailService requestDetailService;

        public ScanningAutoVoidRequestServiceImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.requestDetailService = requestDetailService;
        }

        public async Task Execute(int siteDbGroupId, int timeOut)
        {
            var queue = await autoVoidRequestQueueService.GetValidQueue(siteDbGroupId);
            if (queue == null)
            {
                return;
            }

            var (requests, singleTickets, parlayTickets) =
                await requestDetailService.GetTicketsBySiteDbGroupId(siteDbGroupId, queue.MatchId);
            var (validSingleTickets, validParlayTickets) =
                await requestDetailService.CheckTicketsAllowToVoid(siteDbGroupId, singleTickets, parlayTickets);

            if ((validSingleTickets == null || !validSingleTickets.Any()) && (validParlayTickets == null || !validParlayTickets.Any()))
            {
                await autoVoidRequestQueueService.UpdateQueueStatus(siteDbGroupId, queue.QueueId, ExecuteStatus.Done);
            }

            Debug.WriteLine($"SiteDB - {siteDbGroupId} - {DateTime.Now:hh.mm.ss.ffffff}");
        }
    }
}