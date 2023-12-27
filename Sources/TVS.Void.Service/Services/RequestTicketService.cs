using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TVS.Void.Service.Criteria;
using TVS.Void.Service.Criteria.MultiSiteDb;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services;

public interface RequestTicketService
{
    Task<GroupTicketsInRequest> GetTicketsByRequestsAndSiteDbGroupId(int siteDbGroupId, string requestIds);

    Task<GroupTicketsInRequest> GetTicketInAllRequests();

    Task<CheckStatusTicketInsertAutoVoidQueueOutput> CheckStatusTicketForInsertAutoVoidQueue(int siteDbGroupId,
        IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets,
        IEnumerable<RequestDetailTicketModel> cricketTickets);

    Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets,
        IEnumerable<RequestDetailTicketModel> cricketTickets, IEnumerable<RequestDetailTicketModel> singleTickets14);
}

public class RequestTicketServiceImp : RequestTicketService
{
    private readonly IDynamicRepository repository;

    public RequestTicketServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<GroupTicketsInRequest> GetTicketsByRequestsAndSiteDbGroupId(int siteDbGroupId, string requestIds)
    {
        return await GetTicketsInRequests(siteDbGroupId, requestIds);
    }

    public async Task<GroupTicketsInRequest> GetTicketInAllRequests()
    {
        return await GetTicketsInRequests(null, null);
    }

    private async Task<GroupTicketsInRequest> GetTicketsInRequests(int? siteDbGroupId, string requestIds)
    {
        var criteria = new GetRequestTicketsCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            RequestIds = requestIds,
        };

        var (singleTickets, parlayTickets, cricketTickets) =
            await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>(criteria);

        return new GroupTicketsInRequest
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets
        };
    }

    public async Task<CheckStatusTicketInsertAutoVoidQueueOutput> CheckStatusTicketForInsertAutoVoidQueue(int siteDbGroupId,
            IEnumerable<RequestDetailTicketModel> singleTickets,
            IEnumerable<RequestDetailTicketModel> parlayTickets,
            IEnumerable<RequestDetailTicketModel> cricketTickets)
    {
        var criteria = new CheckStatusTicketCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            SingleTicketsJson = JsonSerializer.Serialize(singleTickets),
            ParlayTicketsJson = JsonSerializer.Serialize(parlayTickets),
            CricketTicketsJson = JsonSerializer.Serialize(cricketTickets)
        };
        var mappingTypes = new[]
        {
            typeof (RequestModel),
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel),
            typeof (RequestDetailTicketModel),
        };
        var objectsArray = await repository.FetchMultipleAsync(criteria, mappingTypes);

        var outputData = new CheckStatusTicketInsertAutoVoidQueueOutput
        {
            ValidRequests = objectsArray[0].Cast<RequestModel>(),
            InvalidSingleTickets = objectsArray[1].Cast<RequestDetailTicketModel>(),
            InvalidParlayTickets = objectsArray[2].Cast<RequestDetailTicketModel>(),
            InvalidCricketTickets = objectsArray[3].Cast<RequestDetailTicketModel>()
        };

        return outputData;
    }

    public async Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets,
        IEnumerable<RequestDetailTicketModel> cricketTickets, IEnumerable<RequestDetailTicketModel> singleTickets14)
    {
        var criteria = new UpdateIsProcessedForRequestTicketsCriteria
        {
            SingleTicketsJson = JsonSerializer.Serialize(singleTickets.Union(singleTickets14)),
            ParlayTicketsJson = JsonSerializer.Serialize(parlayTickets),
            CricketTicketsJson = JsonSerializer.Serialize(cricketTickets)
        };

        await repository.ExecuteAsync(criteria);
    }
}