using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Controllers.Web
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class MasterController : Controller
    {
        private readonly IVendorCategory _vendorCategoryService;

        public MasterController(IVendorCategory vendorCategoryService)
        {
            _vendorCategoryService = vendorCategoryService;
        }
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

        // GET: /Master/Budget
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        public async Task<IActionResult> Budget()
        {
            var vendorCategories = await _vendorCategoryService.GetAllAsync();
            ViewBag.VendorCategories = vendorCategories;
            return View();
        }
    }
}