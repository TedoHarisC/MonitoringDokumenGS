using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonitoringDokumenGS.Controllers.Web
{
    [Authorize]
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
