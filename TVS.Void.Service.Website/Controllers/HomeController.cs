using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TicketVoidingSystem.Models;
using TicketVoidingSystem.Services;

namespace TVS.Void.Service.Website.Controllers;

[ExcludeFromCodeCoverage(Justification = "non-logic")]
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
            $"{{{nameof(AppSettingModel.IsCheckTicketStatusJobExecuting)}: {AppSettingService.AppSetting.IsCheckTicketStatusJobExecuting}," +
            $" {nameof(AppSettingModel.IsScanAutoVoidRequestJobExecuting)}: {AppSettingService.AppSetting.IsScanAutoVoidRequestJobExecuting}," +
            $" {nameof(AppSettingModel.IsVoidJobExecuting)}: {AppSettingService.AppSetting.IsVoidJobExecuting} - {DateTimeOffset.Now}}}";
        logger.LogInformation(jobStatus);
        return Ok("Ok-200-TVS.Void.Service");
    }
}