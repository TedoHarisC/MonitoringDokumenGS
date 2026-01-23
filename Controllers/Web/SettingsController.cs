using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonitoringDokumenGS.Controllers.Web
{
    [Authorize(Roles = "SUPER_ADMIN")]
    public class SettingsController : Controller
    {
        // GET : /Settings/AdminUsers
        public IActionResult AdminUsers()
        {
            return View();
        }

        // GET : /Settings/AuditLog
        public IActionResult AuditLog()
        {
            return View();
        }
    }
}