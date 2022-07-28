using Microsoft.AspNetCore.Mvc;

namespace TVS.Void.Service.Website.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Ok-200-TVS.Void.Service");
        }
    }
}