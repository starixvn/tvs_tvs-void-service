using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using TicketVoidingSystem.Models;

namespace TicketVoidingSystem.Services;

[ExcludeFromCodeCoverage(Justification = "non-logic")]
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
                SignalRChannelPrefix = configuration.GetValue<string>($"{nameof(RedisConfig)}:{nameof(RedisConfig.SignalRChannelPrefix)}")
            },
            CategorySentryLog = configuration.GetValue<string>(nameof(AppSettingModel.CategorySentryLog)),
            SentryClientKey = configuration.GetValue<string>(nameof(AppSettingModel.SentryClientKey)),
            IgnoreSiteDb = configuration.GetValue<string>(nameof(AppSettingModel.IgnoreSiteDb)),
            IsPRODEnvironment = configuration.GetValue<bool>(nameof(AppSettingModel.IsPRODEnvironment))
        };

        AppSetting = setting;
    }
}