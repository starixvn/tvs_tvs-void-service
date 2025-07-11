using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface InsertPreVoidingDataStage
{
    Task<string> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestDetailTicketModel> groupTickets, string voidDataXml, IList<string> logData);

    IEnumerable<RequestDetailTicketModel> MapVoidingTypeToTickets(CheckTicketValidationOutput validation);
}

public class InsertPreVoidingDataStageImp(
    RequestDetailService requestDetailService,
    TicketVoidingQueueService ticketVoidingQueueService,
    RequestTicketService requestTicketService,
    SignalRService notificationService)
    : InsertPreVoidingDataStage
{
    public async Task<string> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue, IEnumerable<RequestDetailTicketModel> groupTickets, string voidDataXml, IList<string> logData)
    {
        if (!groupTickets.Any())
        {
            return "No ticket found.";
        }

        var requestsDistinctJson = DistinctRequestsListJson(groupTickets);
        logData.Add($"{nameof(DistinctRequestsListJson)}: {requestsDistinctJson}");
        var voidingHistoryList = await requestDetailService.InsertVoidingHistory(requestsDistinctJson, siteDbGroupId, queue.BroadcastSessionId, voidDataXml);
        logData.Add($"{nameof(voidingHistoryList)}: {JsonSerializer.Serialize(voidingHistoryList)}");

        await notificationService.UpdateRequestRunningToAllUser(voidingHistoryList);
        await ticketVoidingQueueService.InsertQueueForRequests(siteDbGroupId, voidingHistoryList, groupTickets);
        logData.Add($"InsertQueues-OK - {nameof(groupTickets)}Count: {groupTickets.Count()}");

        await requestTicketService.UpdateIsProcessed(groupTickets);
        logData.Add($"{nameof(RequestTicketService.UpdateIsProcessed)}-AfterInsertVoidQueue-OK");

        return string.Empty;
    }

    private static string DistinctRequestsListJson(IEnumerable<RequestDetailTicketModel> groupTickets)
    {
        var notCareMatchInRequests = groupTickets
            .Select(x => new RequestDetail { RequestId = x.RequestId }).DistinctBy(x => x.RequestId);

        return JsonSerializer.Serialize(notCareMatchInRequests);
    }

    public IEnumerable<RequestDetailTicketModel> MapVoidingTypeToTickets(CheckTicketValidationOutput validation)
    {
        var groupTickets = new List<RequestDetailTicketModel>();

        AddTicketsToGroup(groupTickets, validation.VoidSingleTickets, VoidingTypeConstant.Single);
        AddTicketsToGroup(groupTickets, validation.VoidParlayTickets, VoidingTypeConstant.Parlay);
        AddTicketsToGroup(groupTickets, validation.CancelCOTickets, VoidingTypeConstant.CancelCashOut);
        AddTicketsToGroup(groupTickets, validation.VoidCricketTickets, VoidingTypeConstant.Cricket);
        AddTicketsToGroup(groupTickets, validation.VoidSingleTickets14, VoidingTypeConstant.Single14);
        AddTicketsToGroup(groupTickets, validation.VoidParlayTickets14, VoidingTypeConstant.Parlay14);

        AddTicketsToGroup(groupTickets, validation.RecoverSingleTickets, VoidingTypeConstant.RecoverSingle);
        AddTicketsToGroup(groupTickets, validation.RecoverParlayTickets, VoidingTypeConstant.RecoverParlay);
        AddTicketsToGroup(groupTickets, validation.RecoverSubParlayTickets, VoidingTypeConstant.RecoverSubParlay);
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