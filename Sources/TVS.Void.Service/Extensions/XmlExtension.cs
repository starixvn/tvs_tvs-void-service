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

            return $"<root>{stringBuilder.ToString().Trim()}</root>";
        }

        public static string ToVoidServiceXml(this IEnumerable<VoidServiceModel> models)
        {
            if (models == null || !models.Any())
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            foreach (var model in models)
            {
                var line = $"<r RequestId='{model.RequestId}' IssueTypeId='{model.IssueTypeId}' MatchId='{model.MatchId}' SiteDbGroupId='{model.SiteDbGroupId}'/>";
                stringBuilder.AppendLine(line);
            }

            return $"<root>{stringBuilder.ToString().Trim()}</root>";
        }

        public static string ToAfterVoidDataXml(this IEnumerable<AfterVoidServiceModel> afterVoidData)
        {
            if (afterVoidData == null || !afterVoidData.Any())
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            foreach (var item in afterVoidData)
            {
                var line = $"<r RequestId='{item.RequestId}' VoidingHistoryId='{item.VoidingHistoryId}' MatchId='{item.MatchId}' NegativeBalanceUsers='{item.NegativeBalanceUsers}'/>";
                stringBuilder.AppendLine(line);
            }

            return $"<root>{stringBuilder.ToString().Trim()}</root>";
        }
    }
}