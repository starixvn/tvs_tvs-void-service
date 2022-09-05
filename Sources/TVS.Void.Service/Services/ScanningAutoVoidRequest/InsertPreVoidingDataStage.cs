using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Task<string> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets,
            IEnumerable<RequestDetailTicketModel> validVoidParlayTickets, StringBuilder logData);
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

        public async Task<string> Execute(int siteDbGroupId, AutoVoidRequestQueueModel queue,
            IEnumerable<RequestDetailTicketModel> validVoidSingleTickets, IEnumerable<RequestDetailTicketModel> validVoidParlayTickets, StringBuilder logData)
        {
            if (!validVoidSingleTickets.Any() && !validVoidParlayTickets.Any())
            {
                return "No ticket found.";
            }

            var validVoidServiceList = InitVoidServiceData(siteDbGroupId, validVoidSingleTickets, validVoidParlayTickets);
            var insertedVoidServiceList = await requestDetailService.InsertVoidingHistory(queue.BroadcastSessionId, validVoidServiceList);
            logData.AppendLine($"{nameof(InsertPreVoidingDataStage)}: {JsonSerializer.Serialize(new { validVoidServiceList, insertedVoidServiceList })}");

            await NotifyRequestRunningToAllUser(validVoidServiceList);
            await InsertTicketVoidingQueue(siteDbGroupId, insertedVoidServiceList, validVoidSingleTickets, validVoidParlayTickets);

            return string.Empty;
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

            var finalVoidServiceList = AssignMatchIdForRequests(validVoidServiceList, validVoidSingleTickets, validVoidParlayTickets);
            return finalVoidServiceList;
        }

        private static IEnumerable<VoidServiceModel> AssignMatchIdForRequests(IEnumerable<VoidServiceModel> voidServiceModels,
            IEnumerable<RequestDetailTicketModel> singleTickets, IEnumerable<RequestDetailTicketModel> parlayTickets)
        {
            var finalList = voidServiceModels.ToList();
            finalList.ForEach(voidService =>
            {
                var distinctMatchIdFromSingleTicketsInRequest = singleTickets.Where(x => x.RequestId == voidService.RequestId).Select(x => x.MatchId).Distinct();
                var distinctMatchIdFromParlayTicketsInRequest = parlayTickets.Where(x => x.RequestId == voidService.RequestId).Select(x => x.MatchId).Distinct();
                var distinctMatchIdInRequest = distinctMatchIdFromSingleTicketsInRequest.Union(distinctMatchIdFromParlayTicketsInRequest);
                voidService.MatchId = GetMatchIdFromVoidTickets(voidService.IssueTypeId, distinctMatchIdInRequest);
            });

            return finalList;
        }

        private static int GetMatchIdFromVoidTickets(int issueTypeId, IEnumerable<int> distinctMatchIdInRequest)
        {
            const int parlayMatchId = 29;
            if (issueTypeId == IssueTypeConstant.AbnormalAccount || distinctMatchIdInRequest.Count() > 1 || distinctMatchIdInRequest.Any(x => x == parlayMatchId))
            {
                return 0;
            }

            return distinctMatchIdInRequest.First();
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