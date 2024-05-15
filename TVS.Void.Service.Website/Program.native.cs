using Fanex.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Website.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.LoadConfig(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddFanexData(builder.Configuration);
builder.Services.RegisterFanexExceptionHandler();
builder.Services.RegisterSentry();
builder.Services.RegisterONEbookUM(builder.Configuration);
builder.Services.RegisterSignalR();
builder.Services.RegisterBusinessService();
builder.Services.RegisterQuartzScheduler();

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