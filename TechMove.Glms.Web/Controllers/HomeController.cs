using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TechMove.Glms.Web.Models;

namespace TechMove.Glms.Web.Controllers
{
    public class HomeController : Controller
    {
        ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> loggerParam)
        {
            logger = loggerParam;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ErrorViewModel evm = new ErrorViewModel();
            evm.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View(evm);
        }
    }
}
