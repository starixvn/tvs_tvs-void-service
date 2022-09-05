using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface GetTicketsStage
    {
        Task<(bool IsContinueFlow, GetTicketsStageOutput OutputData)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, StringBuilder logData);
    }

    public class GetTicketsStageImp : GetTicketsStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly RequestDetailService requestDetailService;

        public GetTicketsStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.requestDetailService = requestDetailService;
        }

        public async Task<(bool, GetTicketsStageOutput)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, StringBuilder logData)
        {
            var (singleTickets, parlayTickets) = await requestDetailService.GetTicketsBySiteDbGroupId(siteDbGroupId, queue);
            logData?.AppendLine($"{nameof(GetTicketsStage)}: {JsonSerializer.Serialize(new { singleTickets, parlayTickets })}");
            if (!singleTickets.Any() && !parlayTickets.Any())
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, null, ExecuteStatus.Done, null, "No ticket found.");

                return (false, null);
            }

            var outputData = new GetTicketsStageOutput
            {
                SingleTickets = singleTickets,
                ParlayTickets = parlayTickets
            };
            return (true, outputData);
        }
    }

    public class GetTicketsStageOutput
    {
        public IEnumerable<RequestDetailTicketModel> SingleTickets { get; set; }
        public IEnumerable<RequestDetailTicketModel> ParlayTickets { get; set; }
    }
}