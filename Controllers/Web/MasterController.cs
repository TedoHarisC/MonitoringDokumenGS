using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonitoringDokumenGS.Controllers.Web
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class MasterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Master/Vendor
        public IActionResult Vendor()
        {
            return View();
        }

        // GET: /Master/Users
        public IActionResult Users()
        {
            return View();
        }
    }
}