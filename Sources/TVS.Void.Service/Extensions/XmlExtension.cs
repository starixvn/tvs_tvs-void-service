using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Extensions
{
    public static class XmlExtension
    {
        public static string ToTicketsXml(this IEnumerable<RequestDetailTicketModel> tickets)
        {
            if (tickets == null || !tickets.Any())
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            foreach (var ticket in tickets)
            {
                var line = $"<Ticket TransId='{ticket.TransId}' MainTransId='{ticket.MainTransId}' IssueTypeId='{ticket.IssueTypeId}' MatchId='{ticket.MatchId}'/>";
                stringBuilder.AppendLine(line);
            }

            return $"<Tickets>{stringBuilder.ToString().Trim()}</Tickets>";
        }

        public static string ToRequestInfoXml(this IEnumerable<RequestDetailModel> requests)
        {
            if (requests == null || !requests.Any())
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            foreach (var request in requests)
            {
                var line = $"<Request RequestId='{request.RequestId}' IssueTypeId='{request.IssueTypeId}'/>";
                stringBuilder.AppendLine(line);
            }

            return $"<Requests>{stringBuilder.ToString().Trim()}</Requests>";
        }
    }
}