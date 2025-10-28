using System.Collections.Generic;

namespace TVS.Void.Service.Models;

public class CheckTicketValidationOutput
{
    //// Void
    public IEnumerable<RequestDetailTicketModel> VoidSingleTickets { get; set; }

    public IEnumerable<RequestDetailTicketModel> VoidParlayTickets { get; set; }

    public IEnumerable<RequestDetailTicketModel> VoidCricketTickets { get; set; }

    public IEnumerable<RequestDetailTicketModel> CancelCOTickets { get; set; }

    public IEnumerable<RequestDetailTicketModel> VoidSingleTickets14 { get; set; }

    public IEnumerable<RequestDetailTicketModel> VoidParlayTickets14 { get; set; }

    public IEnumerable<RequestDetailTicketModel> VoidHybridParlayTickets { get; set; }

    //// re settle
    public IEnumerable<RequestDetailTicketModel> SingleTicketsResettle { get; set; }

    public IEnumerable<RequestDetailTicketModel> ParlayTicketsResettle { get; set; }

    public IEnumerable<RequestDetailTicketModel> CricketTicketsResettle { get; set; }

    //// Recover
    public IEnumerable<RequestDetailTicketModel> RecoverSingleTickets { get; set; }

    public IEnumerable<RequestDetailTicketModel> RecoverParlayTickets { get; set; }

    public IEnumerable<RequestDetailTicketModel> RecoverSubParlayTickets { get; set; }

    public IEnumerable<RequestDetailTicketModel> RecoverCricketTickets { get; set; }

    public IEnumerable<RequestDetailTicketModel> RecoverSingleTickets14 { get; set; }

    public IEnumerable<RequestDetailTicketModel> RecoverParlayTickets14 { get; set; }

    public IEnumerable<RequestDetailTicketModel> RecoverHybridParlayTickets { get; set; }
}