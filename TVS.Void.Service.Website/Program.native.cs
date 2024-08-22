using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Website.Utilities;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.
services.LoadConfig(builder.Configuration);
services.AddControllers();
services.AddMemoryCache();
services.AddFanexSql(builder.Configuration);
services.RegisterFanexExceptionHandler();
services.RegisterSentry();
services.RegisterONEbookUM(builder.Configuration);
services.RegisterSignalR();
services.RegisterBusinessService();
services.RegisterQuartzScheduler();

// Build app
var app = builder.Build();
app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<NotificationHub>("/SignalR/NotificationHub");

app.Run();