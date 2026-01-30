using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MonitoringDokumenGS.Models;
using System.Security.Claims;

namespace MonitoringDokumenGS.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Check user role and redirect to appropriate dashboard
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var isAdmin = userRoles.Contains("SUPER_ADMIN") || userRoles.Contains("ADMIN");

        if (isAdmin)
        {
            return View("Index"); // Admin dashboard
        }
        else
        {
            return View("UserDashboard"); // User/Vendor dashboard
        }
    }

    // Explicit action for admin dashboard
    [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
    public IActionResult AdminDashboard()
    {
        return View("Index");
    }

    // Explicit action for user dashboard
    public IActionResult UserDashboard()
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
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
