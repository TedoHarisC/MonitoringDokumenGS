using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonitoringDokumenGS.Controllers.Web
{
    [Authorize]
    public class InvoiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}