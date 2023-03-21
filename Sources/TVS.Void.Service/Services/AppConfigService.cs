using System.Diagnostics.CodeAnalysis;
using TVS.Void.Service.Models;

namespace TVS.Void.Service.Services;

[ExcludeFromCodeCoverage]
public static class AppConfigService
{
    public static ConfigSettingModel AppConfig { get; set; }
}