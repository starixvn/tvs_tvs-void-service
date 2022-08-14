using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface GetTicketsStage
    {
        Task<(bool IsContinueFlow, GetTicketsStageOutput OutputData, string LogData)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue);
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

        public (IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>) OutputData { get; private set; }

        public async Task<(bool, GetTicketsStageOutput, string)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue)
        {
            var (singleTickets, parlayTickets) = await requestDetailService.GetTicketsBySiteDbGroupId(siteDbGroupId, queue);
            var logData = $"{nameof(GetTicketsStage)}: {JsonSerializer.Serialize(new { singleTickets, parlayTickets })}";
            if (!singleTickets.Any() && !parlayTickets.Any())
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, "No ticket found.", null);

                return (false, null, logData);
            }

            var outputData = new GetTicketsStageOutput
            {
                SingleTickets = singleTickets,
                ParlayTickets = parlayTickets
            };
            return (true, outputData, logData);
        }
    }

    public class GetTicketsStageOutput
    {
        public IEnumerable<RequestDetailTicketModel> SingleTickets { get; set; }
        public IEnumerable<RequestDetailTicketModel> ParlayTickets { get; set; }
    }
}