using System;
using System.Collections.Generic;
using System.Linq;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Extension;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface CreateVoidDataXmlStage
{
    string ExecuteForAutoVoid(bool isAutoVoid, CheckTicketValidationOutput validation);
}

public class CreateVoidDataXmlStageImp : CreateVoidDataXmlStage
{
    public string ExecuteForAutoVoid(bool isAutoVoid, CheckTicketValidationOutput validation)
    {
        return isAutoVoid ? ExecuteForAutoVoid(validation) : string.Empty;
    }

#pragma warning disable S1541 // Methods and properties should not be too complex

    private static string ExecuteForAutoVoid(CheckTicketValidationOutput validation)
#pragma warning restore S1541 // Methods and properties should not be too complex
    {
        var voidTickets = new List<RequestDetailTicketModel>();
        voidTickets.AddRange(validation.VoidSingleTickets ?? new List<RequestDetailTicketModel>());
        voidTickets.AddRange(validation.VoidParlayTickets ?? new List<RequestDetailTicketModel>());
        voidTickets.AddRange(validation.VoidCricketTickets ?? new List<RequestDetailTicketModel>());
        voidTickets.AddRange(validation.VoidSingleTickets14 ?? new List<RequestDetailTicketModel>());
        voidTickets.AddRange(validation.VoidParlayTickets14 ?? new List<RequestDetailTicketModel>());
        var cancelCashOutTickets = validation.CancelCOTickets ?? new List<RequestDetailTicketModel>();
        var totalTickets = voidTickets.Union(cancelCashOutTickets);

        var isHasSettledTicket = voidTickets.Exists(x => x.StatusId is TicketStatus.WonId or TicketStatus.DrawId or TicketStatus.LoseId);

        var voidFromTime = DateTime.MinValue;
        var voidToTime = DateTime.MinValue;
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.MinValue;

        if (voidTickets.Any())
        {
            voidFromTime = voidTickets.Select(item => item.TransDate)
                .Union(cancelCashOutTickets.Select(item => item.CheckTimeCancelCashOut)).Min().ZoneManila();
            voidToTime = voidTickets.Select(item => item.TransDate)
                .Union(cancelCashOutTickets.Select(item => item.CheckTimeCancelCashOut)).Max().ZoneManila();

            fromDate = voidTickets.Min(item => item.WinLostDate).ZoneManila();
            toDate = voidTickets.Select(item => item.WinLostDate)
                .Union(cancelCashOutTickets.Select(item => item.CheckTimeCancelCashOut)).Max().ZoneManila();
        }
        else
        {
            if (cancelCashOutTickets.Any())
            {
                voidFromTime = cancelCashOutTickets.Min(item => item.CheckTimeCancelCashOut).ZoneManila();
                voidToTime = cancelCashOutTickets.Max(item => item.CheckTimeCancelCashOut).ZoneManila();

                fromDate = cancelCashOutTickets.Min(item => item.CheckTimeCancelCashOut).ZoneManila();
                toDate = cancelCashOutTickets.Max(item => item.CheckTimeCancelCashOut).ZoneManila();
            }
        }

        return "<VoidData CreatorName='starixIT' " +
               $"VoidFromTime='{voidFromTime.ToString(FormatKey.LongDateFormatTimeWithSlash)}' " +
               $"VoidToTime='{voidToTime.ToString(FormatKey.LongDateFormatTimeWithSlash)}' " +
               $"VoidFromDate='{fromDate.ToString(FormatKey.ShortDateFormatWithSlash)}' " +
               $"VoidToDate='{toDate.ToString(FormatKey.ShortDateFormatWithSlash)}' " +
               $"IsRunAutoAfterVoid='{true}' " +
               $"TotalTicket='{totalTickets.Count()}' " +
               $"IsHasSettledTicket='{isHasSettledTicket}' />";
    }
}