using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services.Notification;

namespace TVS.Void.Service.Services;

public interface SignalRService
{
    Task UpdateRequestRunningToAllUser(IEnumerable<VoidingHistoryModel> voidingHistoryList);

    void UpdateVoidFailedToUser(int requestId, IList<TicketVoidingQueueModel> voidQueueResults);

    Task UpdateRequestFailedToAllUser(int requestId);
}

public class SignalRServiceImp(
    IHubContext<NotificationHub, ClientEvent> hubContext,
    LoggingService loggingService,
    NotificationService notificationService)
    : SignalRService
{
    public async Task UpdateRequestRunningToAllUser(IEnumerable<VoidingHistoryModel> voidingHistoryList)
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

    public void UpdateVoidFailedToUser(int requestId, IList<TicketVoidingQueueModel> voidQueueResults)
    {
        var firstQueue = voidQueueResults.FirstOrDefault();
        var isRecoverQueue = firstQueue != null &&
                             VoidingTypeConstant.VoidingTypeRecover.Contains(firstQueue.VoidingTypeId);

        notificationService.ExecuteBackground(() => notificationService.InsertNotification(NotificationType.Error,
            requestId, MessageHelper.CreateVoidServiceFailed(requestId, voidQueueResults, isRecoverQueue)));
    }

    public async Task UpdateRequestFailedToAllUser(int requestId)
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