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
                var line = $"<r req='{ticket.RequestId}' iss='{ticket.IssueTypeId}' tid='{ticket.TransId}' mid='{ticket.MatchId}' mtid='{ticket.MainTransId}'/>";
                stringBuilder.AppendLine(line);
            }

            return $"<root>{stringBuilder.ToString().Trim()}</root>";
        }
    }
}