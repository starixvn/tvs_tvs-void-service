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
    Task<IEnumerable<TicketVoidingQueueModel>> GetQueues(int siteDbGroupId, int? voidingHistoryId);

    Task InsertQueue(int siteDbGroupId, int requestId, int voidingHistoryId,
        IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);

    Task InsertQueueForRequests(int siteDbGroupId, IEnumerable<VoidingHistoryModel> voidingHistoryList,
        IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets);

    Task<TicketVoidingQueueModel> ProcessVoidQueue(int siteDbGroupId, int queueId);
}

public class TicketVoidingQueueServiceImp : TicketVoidingQueueService
{
    private readonly IDynamicRepository repository;

    public TicketVoidingQueueServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<IEnumerable<TicketVoidingQueueModel>> GetQueues(int siteDbGroupId, int? voidingHistoryId)
    {
        var criteria = new GetTicketVoidingQueueCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            VoidingHistoryId = voidingHistoryId
        };

        var result = await repository.FetchAsync<TicketVoidingQueueModel>(criteria);
        return result;
    }

    public async Task InsertQueue(int siteDbGroupId, int requestId, int voidingHistoryId,
        IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
    {
        var criteria = new InsertTicketVoidingQueueCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            RequestId = requestId,
            VoidingHistoryId = voidingHistoryId,
            SingleTicketsJson = JsonSerializer.Serialize(singleTickets),
            ParlayTicketsJson = JsonSerializer.Serialize(parlayTickets),
            UserId = AutoVoidConstant.ActorId,
            IP = AutoVoidConstant.IPAddress,
        };
        await repository.ExecuteAsync(criteria);
    }

    public async Task InsertQueueForRequests(int siteDbGroupId, IEnumerable<VoidingHistoryModel> voidingHistoryList,
        IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
    {
        foreach (var voidingHistory in voidingHistoryList)
        {
            var singleTicketsInCurrentRequest = singleTickets.Where(x => x.RequestId == voidingHistory.RequestId);
            var parlayTicketsInCurrentRequest = parlayTickets.Where(x => x.RequestId == voidingHistory.RequestId);
            await InsertQueue(siteDbGroupId, voidingHistory.RequestId, voidingHistory.VoidingHistoryId,
                singleTicketsInCurrentRequest, parlayTicketsInCurrentRequest);
        }
    }

    public async Task<TicketVoidingQueueModel> ProcessVoidQueue(int siteDbGroupId, int queueId)
    {
        var criteria = new ProcessVoidQueueCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            QueueId = queueId
        };

        var result = await repository.GetAsync<TicketVoidingQueueModel>(criteria);
        return result;
    }
}