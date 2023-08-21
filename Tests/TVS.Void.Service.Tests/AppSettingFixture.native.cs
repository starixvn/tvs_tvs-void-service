using Microsoft.Extensions.Configuration;
using TicketVoidingSystem.Services;

namespace TVS.Void.Service.Tests;

public class AppSettingFixture
{
    public AppSettingFixture()
    {
        IConfiguration config = new ConfigurationBuilder().Build();
        AppSettingService.InitSetting(config);
    }
}