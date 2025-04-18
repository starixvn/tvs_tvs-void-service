using System;
using System.Collections.Generic;
using System.Linq;
using TicketVoidingSystem.Constants;
using TVS.Void.Service.Constants;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.Notification;

public static class MessageHelper
{
    public static string CreateVoidServiceFailed(int requestId, IList<TicketVoidingQueueModel> voidQueueResults, bool isRecover)
    {
        var action = isRecover ? "recover" : "void";
        var content = $"<strong>Request {requestId}</strong> failed to {action}, please contact IT for support";
        if (voidQueueResults.Any(x => x.VoidingTypeId == VoidingTypeConstant.Cricket && x.StatusId == ExecuteStatus.Failed))
        {
            content = $"{Environment.NewLine}Failed to adjust Net Position of credit members. Consequently, their 'Match Winner' tickets have not been voided";
        }

        return content;
    }
}