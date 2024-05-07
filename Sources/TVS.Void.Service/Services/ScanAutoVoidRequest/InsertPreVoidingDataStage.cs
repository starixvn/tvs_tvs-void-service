using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface InsertPreVoidingDataStage
{
    Task<string> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestDetailTicketModel> groupTickets, IList<string> logData);

    IEnumerable<RequestDetailTicketModel> MapVoidingTypeToTickets(CheckTicketValidationOutput validation);
}

public class InsertPreVoidingDataStageImp : InsertPreVoidingDataStage
{
    private readonly RequestDetailService requestDetailService;
    private readonly TicketVoidingQueueService ticketVoidingQueueService;
    private readonly RequestTicketService requestTicketService;
    private readonly NotificationService notificationService;

    public InsertPreVoidingDataStageImp(RequestDetailService requestDetailService,
        TicketVoidingQueueService ticketVoidingQueueService, RequestTicketService requestTicketService, NotificationService notificationService)
    {
        this.requestDetailService = requestDetailService;
        this.ticketVoidingQueueService = ticketVoidingQueueService;
        this.requestTicketService = requestTicketService;
        this.notificationService = notificationService;
    }

    public async Task<string> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestDetailTicketModel> groupTickets, IList<string> logData)
    {
        if (!groupTickets.Any())
        {
            return "No ticket found.";
        }

        var (multiMatchesInRequestTickets, notCareMatchInRequestTickets) = GroupTicketsByAutoVoidMultiMatches(groupTickets);
        var requestsDistinctJson = DistinctRequestsListJson(multiMatchesInRequestTickets, notCareMatchInRequestTickets);
        logData.Add($"{nameof(DistinctRequestsListJson)}: {requestsDistinctJson}");
        var voidingHistoryList = await requestDetailService.InsertVoidingHistory(requestsDistinctJson, siteDbGroupId, queue.BroadcastSessionId, groupTickets);
        logData.Add($"{nameof(voidingHistoryList)}: {JsonSerializer.Serialize(voidingHistoryList)}");

        await notificationService.NotifyRequestRunningToAllUser(voidingHistoryList);
        await ticketVoidingQueueService.InsertQueueForRequests(siteDbGroupId, voidingHistoryList, multiMatchesInRequestTickets, notCareMatchInRequestTickets);
        logData.Add($"InsertQueues-OK - {nameof(multiMatchesInRequestTickets)}Count: {multiMatchesInRequestTickets.Count()}" +
                    $" - {nameof(notCareMatchInRequestTickets)}Count: {notCareMatchInRequestTickets.Count()}");

        await requestTicketService.UpdateIsProcessed(groupTickets);
        logData.Add($"{nameof(RequestTicketService.UpdateIsProcessed)}-AfterInsertVoidQueue-OK");

        return string.Empty;
    }

    private static (IEnumerable<RequestDetailTicketModel> MultiMatchesInRequestTickets, IEnumerable<RequestDetailTicketModel> NotCareMatchInRequestTickets)
        GroupTicketsByAutoVoidMultiMatches(IEnumerable<RequestDetailTicketModel> groupTickets)
    {
        var abnormalBetRequestTickets = groupTickets.Where(x => x.IssueTypeId == IssueTypeConstant.AbnormalBet);
        var otherRequestTickets = groupTickets.Where(x => x.IssueTypeId != IssueTypeConstant.AbnormalBet);
        return (abnormalBetRequestTickets, otherRequestTickets);
    }

    private static string DistinctRequestsListJson(IEnumerable<RequestDetailTicketModel> multiMatchesInRequestTickets, IEnumerable<RequestDetailTicketModel> notCareMatchInRequestTickets)
    {
        var multiMatchesInRequests = multiMatchesInRequestTickets
            .Select(x => new RequestDetail { RequestId = x.RequestId, MatchId = x.MatchId }).DistinctBy(x => new { x.RequestId, x.MatchId });

        var notCareMatchInRequests = notCareMatchInRequestTickets
            .Select(x => new RequestDetail { RequestId = x.RequestId }).DistinctBy(x => x.RequestId);

        var mergeRequests = multiMatchesInRequests.Union(notCareMatchInRequests);
        return JsonSerializer.Serialize(mergeRequests);
    }

    public IEnumerable<RequestDetailTicketModel> MapVoidingTypeToTickets(CheckTicketValidationOutput validation)
    {
        var groupTickets = new List<RequestDetailTicketModel>();

        AddTicketsToGroup(groupTickets, validation.VoidSingleTickets, VoidingTypeConstant.Single);
        AddTicketsToGroup(groupTickets, validation.VoidParlayTickets, VoidingTypeConstant.Parlay);
        AddTicketsToGroup(groupTickets, validation.VoidCricketTickets, VoidingTypeConstant.Cricket);
        AddTicketsToGroup(groupTickets, validation.VoidSingleTickets14, VoidingTypeConstant.Single14);
        AddTicketsToGroup(groupTickets, validation.VoidParlayTickets14, VoidingTypeConstant.Parlay14);

        AddTicketsToGroup(groupTickets, validation.RecoverSingleTickets, VoidingTypeConstant.RecoverSingle);
        AddTicketsToGroup(groupTickets, validation.RecoverParlayTickets, VoidingTypeConstant.RecoverParlay);
        AddTicketsToGroup(groupTickets, validation.RecoverCricketTickets, VoidingTypeConstant.RecoverCricket);
        AddTicketsToGroup(groupTickets, validation.RecoverSingleTickets14, VoidingTypeConstant.RecoverSingle14);
        AddTicketsToGroup(groupTickets, validation.RecoverParlayTickets14, VoidingTypeConstant.RecoverParlay14);

        return groupTickets;
    }

    private static void AddTicketsToGroup(List<RequestDetailTicketModel> groups, IEnumerable<RequestDetailTicketModel> tickets, int voidingTypeId)
    {
        if (tickets == null || !tickets.Any())
        {
            return;
        }

        var ticketToList = tickets.ToList();
        foreach (var ticket in ticketToList)
        {
            ticket.VoidingTypeId = voidingTypeId;
        }

        groups.AddRange(ticketToList);
    }
}