using System.Collections.Generic;
using System.Linq;
using TicketVoidingSystem.Constants;
using TicketVoidingSystem.Extension;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services.ScanAutoVoidRequest;

public interface CreateVoidDataXmlStage
{
    string Execute(bool isAutoVoid, CheckTicketValidationOutput validation);
}

public class CreateVoidDataXmlStageImp : CreateVoidDataXmlStage
{
    public string Execute(bool isAutoVoid, CheckTicketValidationOutput validation)
    {
        return isAutoVoid ? ExecuteForAutoVoid(validation) : ExecuteForRecover(validation);
    }

    private static string ExecuteForRecover(CheckTicketValidationOutput validation)
    {
        var recoverTickets = new List<RequestDetailTicketModel>();
        recoverTickets.AddRange(validation.RecoverSingleTickets ?? new List<RequestDetailTicketModel>());
        recoverTickets.AddRange(validation.RecoverParlayTickets ?? new List<RequestDetailTicketModel>());
        recoverTickets.AddRange(validation.RecoverCricketTickets ?? new List<RequestDetailTicketModel>());
        recoverTickets.AddRange(validation.RecoverSingleTickets14 ?? new List<RequestDetailTicketModel>());
        recoverTickets.AddRange(validation.RecoverParlayTickets14 ?? new List<RequestDetailTicketModel>());
        recoverTickets.AddRange(validation.RecoverHybridParlayMainTickets ?? new List<RequestDetailTicketModel>());

        return $"<VoidData CreatorName='starixIT' IsRunAutoAfterVoid='{true}' TotalTicket='{recoverTickets.Count}' />";
    }

    private static string ExecuteForAutoVoid(CheckTicketValidationOutput validation)
    {
        var voidTickets = new List<RequestDetailTicketModel>();
        voidTickets.AddRange(validation.VoidSingleTickets ?? new List<RequestDetailTicketModel>());
        voidTickets.AddRange(validation.VoidParlayTickets ?? new List<RequestDetailTicketModel>());
        voidTickets.AddRange(validation.VoidCricketTickets ?? new List<RequestDetailTicketModel>());
        voidTickets.AddRange(validation.VoidSingleTickets14 ?? new List<RequestDetailTicketModel>());
        voidTickets.AddRange(validation.VoidParlayTickets14 ?? new List<RequestDetailTicketModel>());
        voidTickets.AddRange(validation.VoidHybridParlayMainTickets ?? new List<RequestDetailTicketModel>());
        var cancelCashOutTickets = validation.CancelCOTickets ?? new List<RequestDetailTicketModel>();
        var totalTickets = voidTickets.Union(cancelCashOutTickets);
        var isHasSettledTicket = voidTickets.Exists(x => x.StatusId.IsSettled() && !x.BetdaqId.IsCashOut());

        var combineTransTime = voidTickets.Select(x => x.TransDate).Union(cancelCashOutTickets.Select(x => x.CheckTimeCancelCashOut)).DefaultIfEmpty();
        var combineWinLostDate = voidTickets.Select(x => x.WinLostDate).Union(cancelCashOutTickets.Select(x => x.CheckTimeCancelCashOut)).DefaultIfEmpty();

        var voidFromTime = combineTransTime.Min().ZoneManila();
        var voidToTime = combineTransTime.Max().ZoneManila();
        var fromDate = voidTickets.Any() ? voidTickets.Min(item => item.WinLostDate).ZoneManila() : combineWinLostDate.Min().ZoneManila();
        var toDate = combineWinLostDate.Max().ZoneManila();

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