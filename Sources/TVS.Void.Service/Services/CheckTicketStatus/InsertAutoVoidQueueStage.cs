using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.CheckTicketStatus
{
    public interface InsertAutoVoidQueueStage
    {
        Task Execute(int siteDbGroupId, IEnumerable<RequestModel> requests, IList<string> logData);
    }

    public class InsertAutoVoidQueueStageImp : InsertAutoVoidQueueStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;

        public InsertAutoVoidQueueStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
        }

        public async Task Execute(int siteDbGroupId, IEnumerable<RequestModel> requests, IList<string> logData)
        {
            if (requests.Any())
            {
                logData.Add($"{nameof(InsertAutoVoidQueueStage)}[{siteDbGroupId}]: {JsonSerializer.Serialize(requests)}");

                foreach (var request in requests)
                {
                    // DB must check exist queue Status = New => not insert
                    await autoVoidRequestQueueService.InsertQueue(siteDbGroupId, request.RequestId, ExecuteStatus.New, AutoVoidConstant.QueueActionCal);
                }
            }
        }
    }
}