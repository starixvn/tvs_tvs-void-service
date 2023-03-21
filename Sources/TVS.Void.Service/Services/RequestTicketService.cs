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
    Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>,
        IEnumerable<RequestDetailTicketModel>)> GetTicketsByRequestsAndSiteDbGroupId(int siteDbGroupId,
        string requestIds);

    Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>,
        IEnumerable<RequestDetailTicketModel>)> GetTicketInAllRequests();

    Task<CheckStatusTicketInsertAutoVoidQueueOutput> CheckStatusTicketForInsertAutoVoidQueue(int siteDbGroupId,
        IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets,
        IEnumerable<RequestDetailTicketModel> cricketTickets);

    Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> singleTickets,
        IEnumerable<RequestDetailTicketModel> parlayTickets, IEnumerable<RequestDetailTicketModel> cricketTickets);
}

public class RequestTicketServiceImp : RequestTicketService
{
    private readonly IDynamicRepository repository;

    public RequestTicketServiceImp(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>,
            IEnumerable<RequestDetailTicketModel>)> GetTicketsByRequestsAndSiteDbGroupId(int siteDbGroupId, string requestIds)
    {
        return await GetTicketsInRequests(siteDbGroupId, requestIds);
    }

    public async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>)> GetTicketInAllRequests()
    {
        return await GetTicketsInRequests(null, null);
    }

    private async Task<(IEnumerable<RequestDetailTicketModel>, IEnumerable<RequestDetailTicketModel>,
            IEnumerable<RequestDetailTicketModel>)> GetTicketsInRequests(int? siteDbGroupId, string requestIds)
    {
        var criteria = new GetRequestTicketsCriteria
        {
            SiteDbGroupId = siteDbGroupId,
            RequestIds = requestIds,
        };

        var (singleTickets, parlayTickets, cricketTickets) =
            await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>(criteria);

        return (singleTickets, parlayTickets, cricketTickets);
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

    public async Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> singleTickets,
        IEnumerable<RequestDetailTicketModel> parlayTickets, IEnumerable<RequestDetailTicketModel> cricketTickets)
    {
        var criteria = new UpdateIsProcessedForRequestTicketsCriteria
        {
            SingleTicketsJson = JsonSerializer.Serialize(singleTickets),
            ParlayTicketsJson = JsonSerializer.Serialize(parlayTickets),
            CricketTicketsJson = JsonSerializer.Serialize(cricketTickets)
        };

        await repository.ExecuteAsync(criteria);
    }
}