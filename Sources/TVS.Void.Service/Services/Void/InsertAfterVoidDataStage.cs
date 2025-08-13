using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Void;

public interface InsertAfterVoidDataStage
{
    Task Execute(IEnumerable<TicketVoidingQueueModel> voidQueueResults, IList<string> logData);
}

public class InsertAfterVoidDataStageImp(
    RequestDetailService requestDetailService,
    InsertAfterVoidDataPrivateService privateService)
    : InsertAfterVoidDataStage
{
    public async Task Execute(IEnumerable<TicketVoidingQueueModel> voidQueueResults, IList<string> logData)
    {
        if (!voidQueueResults.Any())
        {
            return;
        }

        var firstVoidQueue = voidQueueResults.First();
        var (request, requestDetails) = await requestDetailService.GetRequestById(firstVoidQueue.RequestId);
        var firstRequest = request.First();
        var afterVoidData = new AfterVoidServiceModel
        {
            RequestId = firstVoidQueue.RequestId,
            VoidingHistoryId = firstVoidQueue.VoidingHistoryId,
            MatchId = privateService.GetMatchIdForAfterVoid(firstRequest.IssueTypeId, requestDetails, voidQueueResults),
            NegativeBalanceUsers = privateService.CombineNegativeBalanceUsers(voidQueueResults)
        };

        var voidDataXmlModel = await requestDetailService.GetVoidDataOfVoidingHistory(firstVoidQueue.VoidingHistoryId);
        await requestDetailService.InsertAfterVoidServiceData(voidDataXmlModel.IsRunAutoAfterVoid, afterVoidData, logData);
        await requestDetailService.InsertLetsTalk(firstVoidQueue.RequestId, voidDataXmlModel.IssueTypeId, voidDataXmlModel.BroadcastSessionId, ExecuteStatus.Running, logData);
    }
}