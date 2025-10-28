using Microsoft.Extensions.Configuration;
using TicketVoidingSystem.Models;

namespace TicketVoidingSystem.Services;

public static class AppSettingService
{
    public static AppSettingModel AppSetting { get; set; }

    public static void InitSetting(IConfiguration configuration)
    {
        var setting = new AppSettingModel
        {
            RedisConfig = new RedisConfig
            {
                EndPoint = configuration.GetValue<string>($"{nameof(RedisConfig)}:{nameof(RedisConfig.EndPoint)}"),
                Password = configuration.GetValue<string>($"{nameof(RedisConfig)}:{nameof(RedisConfig.Password)}"),
                DefaultDatabase = configuration.GetValue<int>($"{nameof(RedisConfig)}:{nameof(RedisConfig.DefaultDatabase)}"),
                SignalRChannelPrefix = configuration.GetValue<string>($"{nameof(RedisConfig)}:{nameof(RedisConfig.SignalRChannelPrefix)}"),
                SiteDbChannelPrefix = configuration.GetValue<string>($"{nameof(RedisConfig)}:{nameof(RedisConfig.SiteDbChannelPrefix)}")
            },
            CategorySentryLog = configuration.GetValue<string>(nameof(AppSettingModel.CategorySentryLog)),
            SentryClientKey = configuration.GetValue<string>(nameof(AppSettingModel.SentryClientKey)),
            IgnoreSiteDb = configuration.GetValue<string>(nameof(AppSettingModel.IgnoreSiteDb)),
            IsPRODEnvironment = configuration.GetValue<bool>(nameof(AppSettingModel.IsPRODEnvironment)),
            JackTimeHybridParlayUrl = configuration.GetValue<string>(nameof(AppSettingModel.JackTimeHybridParlayUrl))
        };

        AppSetting = setting;
    }
}