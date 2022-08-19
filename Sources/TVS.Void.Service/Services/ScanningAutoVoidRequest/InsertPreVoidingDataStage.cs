using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanningAutoVoidRequest
{
    public interface InsertPreVoidingDataStage
    {
        Task<(bool IsContinueFlow, string LogData)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets,
            IEnumerable<RequestDetailTicketModel> validVoidParlayTickets);
    }

    public class InsertPreVoidingDataStageImp : InsertPreVoidingDataStage
    {
        private readonly AutoVoidRequestQueueService autoVoidRequestQueueService;
        private readonly RequestDetailService requestDetailService;
        private readonly TicketVoidingQueueService ticketVoidingQueueService;
        private readonly TicketVoidingSystemHttpClientService ticketVoidingSystemHttpClientService;
        private readonly IHubContext<NotificationHub, ClientEvent> hubContext;

        public InsertPreVoidingDataStageImp(AutoVoidRequestQueueService autoVoidRequestQueueService, RequestDetailService requestDetailService,
            TicketVoidingQueueService ticketVoidingQueueService, TicketVoidingSystemHttpClientService ticketVoidingSystemHttpClientService, IHubContext<NotificationHub, ClientEvent> hubContext)
        {
            this.autoVoidRequestQueueService = autoVoidRequestQueueService;
            this.requestDetailService = requestDetailService;
            this.ticketVoidingQueueService = ticketVoidingQueueService;
            this.ticketVoidingSystemHttpClientService = ticketVoidingSystemHttpClientService;
            this.hubContext = hubContext;
        }

        public async Task<(bool, string)> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets)
        {
            if (!validVoidSingleTickets.Any() && !validVoidParlayTickets.Any())
            {
                await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, null, "No ticket found.");

                return (false, null);
            }

            var validVoidServiceList = InitVoidServiceData(siteDbGroupId, validVoidSingleTickets, validVoidParlayTickets);
            var insertedVoidServiceList = await requestDetailService.InsertVoidingHistory(queue.BroadcastSessionId, validVoidServiceList);
            var logData = $"{nameof(InsertPreVoidingDataStage)}: {JsonSerializer.Serialize(new { validVoidServiceList, insertedVoidServiceList })}";

            await NotifyRequestRunningToAllUser(validVoidServiceList);
            await InsertTicketVoidingQueue(siteDbGroupId, insertedVoidServiceList, validVoidSingleTickets, validVoidParlayTickets);
            await autoVoidRequestQueueService.UpdateQueue(siteDbGroupId, queue.QueueId, ExecuteStatus.Done, null, null);
            return (true, logData);
        }

        private static IEnumerable<VoidServiceModel>
            InitVoidServiceData(int siteDbGroupId, IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets)
        {
            var equalityComparer = new VoidServiceModelEqualityComparer();
            var distinctRequestFirst = validVoidSingleTickets.Select(x => new VoidServiceModel
            { RequestId = x.RequestId, IssueTypeId = x.IssueTypeId, SiteDbGroupId = siteDbGroupId }).Distinct(equalityComparer);
            var distinctRequestSecond = validVoidParlayTickets.Select(x => new VoidServiceModel
            { RequestId = x.RequestId, IssueTypeId = x.IssueTypeId, SiteDbGroupId = siteDbGroupId }).Distinct(equalityComparer);
            var validVoidServiceList = distinctRequestFirst.Union(distinctRequestSecond, equalityComparer);

            AssignMatchIdForRequests(validVoidServiceList, validVoidSingleTickets, validVoidParlayTickets);

            return validVoidServiceList;
        }

        private static void AssignMatchIdForRequests(IEnumerable<VoidServiceModel> voidServiceModels,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            const int mixParlayMatch = 29;
            foreach (var voidService in voidServiceModels)
            {
                var ticketsInRequest = singleTickets.Where(x => x.RequestId == voidService.RequestId)
                    .Union(parlayTickets.Where(x => x.RequestId == voidService.RequestId), new RequestDetailTicketModelEqualityComparer());

                voidService.MatchId = (voidService.IssueTypeId is IssueTypeConstant.AbnormalBet or IssueTypeConstant.AbnormalBetRealMember
                                       && !ticketsInRequest.Any(x => x.RequestId == voidService.RequestId && x.MatchId == mixParlayMatch))
                    ? ticketsInRequest.First().MatchId : 0;
            }
        }

        private async Task NotifyRequestRunningToAllUser(IEnumerable<VoidServiceModel> voidServiceModels)
        {
            var runningRequests = voidServiceModels.Select(x => new RequestStatus
            { RequestId = x.RequestId, Status = RequestStatusConstant.Running });
            await ticketVoidingSystemHttpClientService.SignalRUpdateRequestStatusToAllUsers(runningRequests);
            await hubContext.Clients.All.updateRequestStatus(runningRequests);
        }

        private async Task InsertTicketVoidingQueue(int siteDbGroupId, IEnumerable<VoidServiceModel> voidServiceList,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets)
        {
            foreach (var voidService in voidServiceList)
            {
                var singleTicketsInCurrentRequest = validVoidSingleTickets.Where(x => x.RequestId == voidService.RequestId);
                var parlayTicketsInCurrentRequest = validVoidParlayTickets.Where(x => x.RequestId == voidService.RequestId);
                await ticketVoidingQueueService.InsertQueue(siteDbGroupId, voidService.VoidingHistoryId, voidService.RequestId, voidService.IssueTypeId,
                    singleTicketsInCurrentRequest, parlayTicketsInCurrentRequest);
            }
        }
    }
}