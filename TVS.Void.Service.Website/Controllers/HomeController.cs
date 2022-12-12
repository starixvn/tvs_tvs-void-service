using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TVS.Void.Service.Models;
using TVS.Void.Service.Services;

namespace TVS.Void.Service.Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var jobStatus =
                $"{{{nameof(ConfigSettingModel.IsCheckTicketStatusJobExecuting)}: {AppConfigService.AppConfig.IsCheckTicketStatusJobExecuting}," +
                $" {nameof(ConfigSettingModel.IsScanAutoVoidRequestJobExecuting)}: {AppConfigService.AppConfig.IsScanAutoVoidRequestJobExecuting}," +
                $" {nameof(ConfigSettingModel.IsVoidJobExecuting)}: {AppConfigService.AppConfig.IsVoidJobExecuting} - {DateTime.Now}}}";
            logger.LogInformation(jobStatus);
            return Ok("Ok-200-TVS.Void.Service");
        }
    }
}