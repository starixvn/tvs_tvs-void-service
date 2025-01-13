using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services;

public interface NotificationService
{
    Task NotifyRequestRunningToAllUser(IEnumerable<VoidingHistoryModel> voidingHistoryList);

    Task NotifyVoidFailedToUser(int requestId, IList<TicketVoidingQueueModel> voidQueueResults);

    Task NotifyRequestFailedToAllUser(int requestId);
}

public class NotificationServiceImp(IHubContext<NotificationHub, ClientEvent> hubContext, LoggingService loggingService)
    : NotificationService
{
    public async Task NotifyRequestRunningToAllUser(IEnumerable<VoidingHistoryModel> voidingHistoryList)
    {
        try
        {
            var runningRequests = voidingHistoryList.Select(x
                => new RequestStatus { RequestId = x.RequestId, Status = RequestStatusConstant.Running });
            await hubContext.Clients.All.updateRequestStatus(runningRequests);
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync(e);
        }
    }

    public async Task NotifyVoidFailedToUser(int requestId, IList<TicketVoidingQueueModel> voidQueueResults)
    {
        try
        {
            var notifyMessage = new StringBuilder();
            var voidResultHasUserInfo = voidQueueResults.First(x => !string.IsNullOrWhiteSpace(x.UserName));
            notifyMessage.AppendLine($"Failed to void Request ID: {requestId}, please contact IT for support. (Error: VOID_SERVICE).");
            if (voidQueueResults.Any(x => x.VoidingTypeId == VoidingTypeConstant.Cricket && x.StatusId == ExecuteStatus.Failed))
            {
                notifyMessage.AppendLine($" {Messages.FailedApiVoidCricket}");
            }
            await hubContext.Clients.User(voidResultHasUserInfo.UserName).showErrorMessage(notifyMessage.ToString());
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync(e);
        }
    }

    public async Task NotifyRequestFailedToAllUser(int requestId)
    {
        try
        {
            var failedRequests = new List<RequestStatus> { new() { RequestId = requestId, Status = RequestStatusConstant.Failed } };
            await hubContext.Clients.All.updateRequestStatus(failedRequests);
        }
        catch (Exception e)
        {
            await loggingService.ErrorAsync(e);
        }
    }
}