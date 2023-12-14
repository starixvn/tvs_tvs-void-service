namespace TicketVoidingSystem.Models;

public class AppSettingModel
{
    // For execute job
    public bool IsScanAutoVoidRequestJobExecuting { get; set; }

    public bool IsVoidJobExecuting { get; set; }

    public bool IsCheckTicketStatusJobExecuting { get; set; }

    public bool IsAddRequestDuplicatedJobExecuting { get; set; }

    public RedisConfig RedisConfig { get; init; }

    // Sentry
    public string CategorySentryLog { get; init; }

    public string SentryClientKey { get; init; }

    public string IgnoreSiteDb { get; init; }

    public bool IsPRODEnvironment { get; init; }
}

public class RedisConfig
{
    public string EndPoint { get; init; }
    public string Password { get; init; }
    public int DefaultDatabase { get; init; }
    public string SignalRChannelPrefix { get; init; }
}