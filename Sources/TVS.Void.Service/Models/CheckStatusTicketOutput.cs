using System.Collections.Generic;

namespace TVS.Void.Service.Models
{
    public class CheckStatusTicketOutput
    {
        public IEnumerable<RequestDetailTicketModel> ValidVoidSingleTickets { get; set; }
        public IEnumerable<RequestDetailTicketModel> ValidVoidParlayTickets { get; set; }
        public IEnumerable<RequestDetailTicketModel> SingleTicketsResettle { get; set; }
        public IEnumerable<RequestDetailTicketModel> ParlayTicketsResettle { get; set; }
    }
}