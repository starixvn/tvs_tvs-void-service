using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services;

public interface TicketVoidingQueueService
{
    Task<IList<TicketVoidingQueueModel>> GetQueues(int siteDbGroupId, int? voidingHistoryId);

    Task InsertQueue(int siteDbGroupId, int requestId, int voidingHistoryId, IEnumerable<RequestDetailTicketModel> groupTickets);

    Task InsertQueueForRequests(int siteDbGroupId, IEnumerable<VoidingHistoryModel> voidingHistoryList, IEnumerable<RequestDetailTicketModel> groupTickets);

    Task<TicketVoidingQueueModel> ProcessVoidQueue(int siteDbGroupId, int queueId, bool? isCalledNetPosition);
}

public class TicketVoidingQueueServiceImp(IDynamicRepository repository) : TicketVoidingQueueService
{
    public async Task<IList<TicketVoidingQueueModel>> GetQueues(int siteDbGroupId, int? voidingHistoryId)
    {
        var criteria = new GetTicketVoidingQueueCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            VoidingHistoryId = voidingHistoryId
        };

        var voidingQueues = await repository.FetchAsync<TicketVoidingQueueModel>(criteria);

        return voidingQueues.ToList();
    }

    public async Task InsertQueue(int siteDbGroupId, int requestId, int voidingHistoryId, IEnumerable<RequestDetailTicketModel> groupTickets)
    {
        var criteria = new InsertTicketVoidingQueueCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            RequestId = requestId,
            VoidingHistoryId = voidingHistoryId,
            TicketsJson = JsonSerializer.Serialize(groupTickets),
            UserId = AutoVoidConstant.ActorId,
            IP = AutoVoidConstant.IPAddress,
        };
        await repository.ExecuteAsync(criteria);
    }

    public async Task InsertQueueForRequests(int siteDbGroupId, IEnumerable<VoidingHistoryModel> voidingHistoryList, IEnumerable<RequestDetailTicketModel> groupTickets)
    {
        foreach (var voidingHistory in voidingHistoryList)
        {
            var ticketsInCurrentRequest = groupTickets.Where(x => x.RequestId == voidingHistory.RequestId);
            await InsertQueue(siteDbGroupId, voidingHistory.RequestId, voidingHistory.VoidingHistoryId, ticketsInCurrentRequest);
        }
    }

    public async Task<TicketVoidingQueueModel> ProcessVoidQueue(int siteDbGroupId, int queueId, bool? isCalledNetPosition)
    {
        var criteria = new ProcessVoidQueueCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            QueueId = queueId,
            IsCalledNetPosition = isCalledNetPosition
        };

        var result = await repository.GetAsync<TicketVoidingQueueModel>(criteria);
        return result;
    }
}