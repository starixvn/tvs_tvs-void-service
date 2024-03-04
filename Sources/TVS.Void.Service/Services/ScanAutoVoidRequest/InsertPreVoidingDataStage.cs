using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

        var requestIds = string.Join(",", DistinctRequestIdList(groupTickets));
        var voidingHistoryList = await requestDetailService.InsertVoidingHistory(requestIds, siteDbGroupId, queue.BroadcastSessionId, groupTickets);
        logData.Add($"{nameof(voidingHistoryList)}: {JsonSerializer.Serialize(voidingHistoryList)}");

        await notificationService.NotifyRequestRunningToAllUser(voidingHistoryList);
        await ticketVoidingQueueService.InsertQueueForRequests(siteDbGroupId, voidingHistoryList, groupTickets);
        logData.Add("InsertQueues-OK");

        await requestTicketService.UpdateIsProcessed(groupTickets);
        logData.Add($"{nameof(RequestTicketService.UpdateIsProcessed)}-AfterInsertVoidQueue-OK");

        return string.Empty;
    }

    private static IEnumerable<int> DistinctRequestIdList(IEnumerable<RequestDetailTicketModel> groupTickets)
    {
        var requestIdList = groupTickets.Select(x => x.RequestId).Distinct();
        return requestIdList;
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