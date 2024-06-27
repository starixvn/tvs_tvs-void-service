using System.Collections.Generic;

namespace TVS.Void.Service.Models;

public class GroupTicketsInRequest
{
    public IEnumerable<RequestDetailTicketModel> SingleTickets { get; set; }
    public IEnumerable<RequestDetailTicketModel> ParlayTickets { get; set; }
    public IEnumerable<RequestDetailTicketModel> CricketTickets { get; set; }
    public IEnumerable<RequestDetailTicketModel> CancelCOTickets { get; set; }
}