using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace TVS.Void.Service.Website.Controllers;

[ExcludeFromCodeCoverage(Justification = "non-logic")]
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Ok-200-TVS.Void.Service");
    }
}