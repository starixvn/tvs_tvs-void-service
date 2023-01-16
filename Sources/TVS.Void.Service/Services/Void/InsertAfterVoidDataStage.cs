using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void
{
    public interface InsertAfterVoidDataStage
    {
        Task Execute(IEnumerable<TicketVoidingQueueModel> voidQueueResults, IList<string> logData);
    }

    public class InsertAfterVoidDataStageImp : InsertAfterVoidDataStage
    {
        private readonly InsertAfterVoidDataPrivateService privateService;
        private readonly RequestDetailService requestDetailService;

        public InsertAfterVoidDataStageImp(RequestDetailService requestDetailService, InsertAfterVoidDataPrivateService privateService)
        {
            this.requestDetailService = requestDetailService;
            this.privateService = privateService;
        }

        public async Task Execute(IEnumerable<TicketVoidingQueueModel> voidQueueResults, IList<string> logData)
        {
            var firstVoidQueue = voidQueueResults.First();
            var (request, requestDetails) = await requestDetailService.GetRequestById(firstVoidQueue.RequestId);
            var afterVoidData = new AfterVoidServiceModel
            {
                RequestId = firstVoidQueue.RequestId,
                VoidingHistoryId = firstVoidQueue.VoidingHistoryId,
                MatchId = privateService.GetMatchIdForAfterVoid(firstVoidQueue.VoidingHistoryId, requestDetails, voidQueueResults),
                NegativeBalanceUsers = privateService.CombineNegativeBalanceUsers(voidQueueResults)
            };

            var voidDataOfVoidingHistory = await requestDetailService.GetVoidDataOfVoidingHistory(firstVoidQueue.VoidingHistoryId);
            var insertedAfterVoidData = await requestDetailService.InsertAfterVoidServiceData(privateService.IsAutoVoid(request.First(), voidDataOfVoidingHistory), afterVoidData);
            logData.Add($"{nameof(RequestDetailService.InsertAfterVoidServiceData)}: {JsonSerializer.Serialize(insertedAfterVoidData)}");
        }
    }
}