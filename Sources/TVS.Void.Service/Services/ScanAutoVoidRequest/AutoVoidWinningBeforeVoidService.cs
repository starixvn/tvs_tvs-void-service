using System.Collections.Generic;
using System.Threading.Tasks;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface AutoVoidWinningBeforeVoidService
{
    Task<(bool IsValid, string RequestIdsChangedAction)> CheckVoidWinningRequestQueueValid(int siteDbGroupId, AutoVoidRequestQueueModel queue);

    Task<(bool IsContinueFlow, GroupTicketsInRequest OutputData)> GetTickets(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData);
}

public class AutoVoidWinningBeforeVoidServiceImp : AutoVoidWinningBeforeVoidService
{
    private readonly GetTicketsStage getTicketsStage;
    private readonly CheckVoidWinningRequestQueueValidStage checkRequestQueueValidStage;

    public AutoVoidWinningBeforeVoidServiceImp(GetTicketsStage getTicketsStage, CheckVoidWinningRequestQueueValidStage checkRequestQueueValidStage)
    {
        this.getTicketsStage = getTicketsStage;
        this.checkRequestQueueValidStage = checkRequestQueueValidStage;
    }

    public async Task<(bool IsValid, string RequestIdsChangedAction)> CheckVoidWinningRequestQueueValid(int siteDbGroupId, AutoVoidRequestQueueModel queue)
    {
        return await checkRequestQueueValidStage.Execute(siteDbGroupId, queue);
    }

    public async Task<(bool, GroupTicketsInRequest)> GetTickets(int siteDbGroupId, AutoVoidRequestQueueModel queue, IList<string> logData)
    {
        return await getTicketsStage.Execute(siteDbGroupId, queue, logData);
    }
}