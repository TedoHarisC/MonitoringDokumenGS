using Microsoft.AspNetCore.Mvc;

namespace MonitoringDokumenGS.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    // GET: /Auth/Index (Login)
    public IActionResult Index()
    {
        return View();
    }

    // GET: /Auth/Register
    public IActionResult Register()
    {
        return View();
    }
}