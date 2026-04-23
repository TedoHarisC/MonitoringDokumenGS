using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonitoringDokumenGS.Controllers.Web
{
    [Authorize]
    public class TemplateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserPage()
        {
            return View();
        }
    }
}