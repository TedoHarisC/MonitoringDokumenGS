using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using MonitoringDokumenGS.Interfaces;
using Dtos.Login;

namespace MonitoringDokumenGS.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuth _auth;
    private readonly IRoleService _roleService;
    private readonly IUser _userService;


    public AuthController(ILogger<AuthController> logger, IAuth auth, IUser userService, IRoleService roleService)
    {
        _logger = logger;
        _auth = auth;
        _roleService = roleService;
        _userService = userService;
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

    // POST: /Auth/Login (Login)
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var (token, refresh) = await _auth.LoginAsync(request);

            var user = await _userService.GetByUsernameAsync(request.Username);
            var roles = await _roleService.GetUserRolesAsync(user!.UserId);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, request.Username),
                new Claim("userId", request.Username)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Code));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            // jangan redirect, RETURN JSON
            return Ok(new { success = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Unexpected error" });
        }
    }

    // AuthController (WEB)
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Ok(new { success = true });
    }

}