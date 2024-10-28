using Fanex.Data.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Website.Utilities;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.
services.AddTvsConfig(builder.Configuration);
services.AddControllers();
services.AddMemoryCache();
services.AddFanexSql(builder.Configuration);
services.AddFanexExceptionHandler();
services.AddSentry();
services.AddONEbookUM(builder.Configuration);
services.AddTvsSignalR();
services.AddBusinessService();
services.AddQuartzScheduler();

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