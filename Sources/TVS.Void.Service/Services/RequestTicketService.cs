using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using TicketVoidingSystem.Constants;
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
        IEnumerable<RequestDetailTicketModel> cricketTickets);

    Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> groupTickets);
}

public class RequestTicketServiceImp(IDynamicRepository repository) : RequestTicketService
{
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
            RequestIds = requestIds
        };

        var (singleTickets, parlayTickets, cricketTickets, cancelCashOutTickets) =
            await repository.FetchMultipleAsync<RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>(criteria);

        return new GroupTicketsInRequest
        {
            SingleTickets = singleTickets,
            ParlayTickets = parlayTickets,
            CricketTickets = cricketTickets,
            CancelCOTickets = cancelCashOutTickets
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

        var (validRequests, invalidSingleTickets, invalidParlayTickets, invalidCricketTickets) =
            await repository.FetchMultipleAsync<RequestModel, RequestDetailTicketModel, RequestDetailTicketModel, RequestDetailTicketModel>(criteria);

        var outputData = new CheckStatusTicketInsertAutoVoidQueueOutput
        {
            ValidRequests = validRequests,
            InvalidSingleTickets = invalidSingleTickets,
            InvalidParlayTickets = invalidParlayTickets,
            InvalidCricketTickets = invalidCricketTickets
        };

        return outputData;
    }

    public async Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets,
        IEnumerable<RequestDetailTicketModel> cricketTickets)
    {
        var criteria = new UpdateIsProcessedForRequestTicketsCriteria
        {
            //CancelCashOutTicketsJson = Just use in manual void
            SingleTicketsJson = JsonSerializer.Serialize(singleTickets),
            ParlayTicketsJson = JsonSerializer.Serialize(parlayTickets),
            CricketTicketsJson = JsonSerializer.Serialize(cricketTickets)
        };

        await repository.ExecuteAsync(criteria);
    }

    public async Task UpdateIsProcessed(IEnumerable<RequestDetailTicketModel> groupTickets)
    {
        var input = SplitTicketsForUpdateIsProcessed(groupTickets);
        var criteria = new UpdateIsProcessedForRequestTicketsCriteria
        {
            SingleTicketsJson = JsonSerializer.Serialize(input.SingleTickets),
            ParlayTicketsJson = JsonSerializer.Serialize(input.ParlayTickets)
        };

        await repository.ExecuteAsync(criteria);
    }

    private static (IEnumerable<RequestDetailTicketModel> SingleTickets, IEnumerable<RequestDetailTicketModel> ParlayTickets)
        SplitTicketsForUpdateIsProcessed(IEnumerable<RequestDetailTicketModel> groupTickets)
    {
        int[] combineSingleVoidingTypes =
        [
            VoidingTypeConstant.Single, VoidingTypeConstant.Cricket,
            VoidingTypeConstant.CancelCashOut, VoidingTypeConstant.Single14, VoidingTypeConstant.RecoverSingle,
            VoidingTypeConstant.RecoverCricket, VoidingTypeConstant.RecoverSingle14
        ];
        int[] combineParlayVoidingTypes =
        [
            VoidingTypeConstant.Parlay, VoidingTypeConstant.Parlay14,
            VoidingTypeConstant.RecoverParlay, VoidingTypeConstant.RecoverParlay14, VoidingTypeConstant.RecoverSubParlay,
            VoidingTypeConstant.HybridParlay, VoidingTypeConstant.SubHybridParlay, VoidingTypeConstant.MainHybridParlay14,
            VoidingTypeConstant.RecoverHybridParlay, VoidingTypeConstant.RecoverSubHybridParlay, VoidingTypeConstant.RecoverMainHybridParlay14

        ];

        var singleTickets = groupTickets.Where(x => combineSingleVoidingTypes.Contains(x.VoidingTypeId));
        var parlayTickets = groupTickets.Where(x => combineParlayVoidingTypes.Contains(x.VoidingTypeId));

        return (singleTickets, parlayTickets);
    }
}