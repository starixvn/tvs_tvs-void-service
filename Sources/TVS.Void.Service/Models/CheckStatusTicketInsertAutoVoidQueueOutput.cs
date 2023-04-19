using System.Collections.Generic;

namespace TVS.Void.Service.Models;

public class CheckStatusTicketInsertAutoVoidQueueOutput
{
    public IEnumerable<RequestModel> ValidRequests { get; set; }
    public IEnumerable<RequestDetailTicketModel> InvalidSingleTickets { get; set; }
    public IEnumerable<RequestDetailTicketModel> InvalidParlayTickets { get; set; }
    public IEnumerable<RequestDetailTicketModel> InvalidCricketTickets { get; set; }
}