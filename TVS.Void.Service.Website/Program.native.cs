using System;
using Fanex.Caching.Redis;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.DependencyInjection;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Website.Utilities;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.
services.AddTvsConfig(configuration);
services.AddControllers();
services.AddMemoryCache();
services.AddFanexSql(configuration);
services.AddFanexExceptionHandler();
services.AddSentry();
services.AddOneBookUmForService(configuration);
services.AddHealthChecks();
services.AddRedisCache(configuration.GetSection("FanexCachingRedis"));
services.AddTvsSignalR();
services.AddTvsSiteDbStatus();
services.AddBusinessService();
services.AddBackgroundService();

// Build app
var app = builder.Build();
app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapHealthChecks("/healthz");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<NotificationHub>("/SignalR/NotificationHub");

var message = $"Fanex.TVS.Void.Service - Started - {DateTimeOffset.Now}";
Console.WriteLine(message);
await LogService.InfoAsync(message);

await app.RunAsync();

message = $"Fanex.TVS.Void.Service - Shutdown!!! - {DateTimeOffset.Now}";
Console.WriteLine(message);
await LogService.InfoAsync(message);