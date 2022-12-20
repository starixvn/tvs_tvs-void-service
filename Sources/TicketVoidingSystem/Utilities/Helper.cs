using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TicketVoidingSystem.Constants;

namespace TicketVoidingSystem.Utilities
{
    [ExcludeFromCodeCoverage]
    public static class Helper
    {
        public static string GetSettingKeyBySiteDbGroupId(int siteDbGroupId, string spName)
        {
            var serverName = SiteDbGroupConstant.ServerList.First(x => x.Id == siteDbGroupId).ServerName;

            return $"{serverName}.{spName}";
        }
    }
}