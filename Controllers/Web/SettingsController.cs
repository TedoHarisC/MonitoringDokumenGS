using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.ViewModels.Settings;
using BCryptNet = BCrypt.Net.BCrypt;

namespace MonitoringDokumenGS.Controllers.Web
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IUser _userService;

        public SettingsController(ApplicationDBContext context, IUser userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET : /Settings/AdminUsers
        [Authorize(Roles = "SUPER_ADMIN")]
        public IActionResult AdminUsers()
        {
            return View();
        }

        // GET : /Settings/AuditLog
        [Authorize(Roles = "SUPER_ADMIN")]
        public IActionResult AuditLog()
        {
            return View();
        }

        // GET : /Settings/ProfileDetails
        public async Task<IActionResult> ProfileDetails()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return RedirectToAction("Index", "Auth");

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Index", "Auth");

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var vm = new ProfileDetailsViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                VendorName = user.VendorName,
                LastLoginAt = user.LastLoginAt,
                Roles = roles,
                ChangePassword = new ChangePasswordViewModel()
            };

            return View(vm);
        }

        // POST : /Settings/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel model)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return RedirectToAction("Index", "Auth");

            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.isDeleted);
            if (userEntity == null)
                return RedirectToAction("Index", "Auth");

            if (!ModelState.IsValid)
            {
                return await ReturnProfileWithModelErrors(userId, model);
            }

            if (!BCryptNet.Verify(model.CurrentPassword, userEntity.PasswordHash))
            {
                ModelState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword), "Current password is incorrect.");
                return await ReturnProfileWithModelErrors(userId, model);
            }

            if (!string.Equals(model.NewPassword, model.ConfirmNewPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(ChangePasswordViewModel.ConfirmNewPassword), "Confirmation password does not match.");
                return await ReturnProfileWithModelErrors(userId, model);
            }

            userEntity.PasswordHash = BCryptNet.HashPassword(model.NewPassword);
            userEntity.SecurityStamp = Guid.NewGuid();
            userEntity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Logout user after changing password
            TempData["AuthInfoTitle"] = "Logged out";
            TempData["AuthInfoMessage"] = "Password berhasil diganti. Demi keamanan, Anda otomatis logout. Silakan login kembali.";
            TempData["AuthInfoIcon"] = "info";

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Auth");
        }

        private async Task<IActionResult> ReturnProfileWithModelErrors(Guid userId, ChangePasswordViewModel changePassword)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Index", "Auth");

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var vm = new ProfileDetailsViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                VendorName = user.VendorName,
                LastLoginAt = user.LastLoginAt,
                Roles = roles,
                ChangePassword = changePassword
            };

            return View("ProfileDetails", vm);
        }
    }
}