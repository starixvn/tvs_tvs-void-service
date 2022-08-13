using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketVoidingSystem.SignalR;
using TVS.Void.Service.Website.Utilities;

namespace TVS.Void.Service.Website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.LoadConfig();
            services.AddControllers();
            services.RegisterFanexData(Configuration);
            services.RegisterFanexExceptionHandler();
            services.RegisterSentry(Configuration);
            services.RegisterONEbookUM(Configuration);
            services.RegisterSignalR(Configuration);
            services.RegisterBusinessService();
            services.RegisterQuartzScheduler();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<NotificationHub>("/SignalR/NotificationHub");
            });
        }
    }
}