using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;
using Dtos.Login;
using Dtos.Register;
using System.Security.Claims;

namespace MonitoringDokumenGS.Controllers.API
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuth _auth;
        private readonly IUser _userService;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(IAuth auth, IUser userService, ILogger<AuthApiController> logger)
        {
            _auth = auth;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid) return this.ValidationErrorResponse();

            try
            {
                var created = await _auth.RegisterAsync(request);
                if (!created)
                    return this.ErrorResponse("Username or email already exists", 400);

                return this.OkResponse<object>(null, "Registration successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user {Username}", request?.Username);
                var errors = ExceptionExtensions.GetAllMessages(ex);
                return this.ErrorResponse(ex.Message, 500, errors);
            }
        }

        /// <summary>
        /// Login and receive JWT + refresh token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid) return this.ValidationErrorResponse();

            try
            {
                var (token, refresh) = await _auth.LoginAsync(request);
                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(refresh))
                    return this.ErrorResponse("Invalid credentials", 401);

                return this.OkResponse(new { token, refreshToken = refresh });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Username}", request?.Username);
                var errors = ExceptionExtensions.GetAllMessages(ex);
                return this.ErrorResponse("An unexpected error occurred", 500, errors);
            }
        }

        /// <summary>
        /// Refresh JWT using a refresh token.
        /// </summary>
        public class RefreshRequest { public string RefreshToken { get; set; } = string.Empty; }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
                return this.ErrorResponse("Refresh token is required", 400);

            try
            {
                var (token, refresh) = await _auth.RefreshTokenAsync(request.RefreshToken);
                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(refresh))
                    return this.ErrorResponse("Invalid or expired refresh token", 401);

                return this.OkResponse(new { token, refreshToken = refresh });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return this.ErrorResponse("An unexpected error occurred", 500);
            }
        }

        /// <summary>
        /// Logout by revoking refresh token.
        /// </summary>
        public class LogoutRequest { public string RefreshToken { get; set; } = string.Empty; }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
                return this.ErrorResponse("Refresh token is required", 400);

            try
            {
                var ok = await _auth.LogoutAsync(request.RefreshToken);
                if (!ok) return this.ErrorResponse("Refresh token not found or already revoked", 404);
                return this.OkResponse<object>(null, "Successfully logged out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout error");
                return this.ErrorResponse("An unexpected error occurred", 500);
            }
        }

        /// <summary>
        /// Generate reset token and return it (in production send via email instead).
        /// </summary>
        public class GenerateResetRequest { public string Username { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; }

        [HttpPost("generate-reset")]
        public async Task<IActionResult> GenerateReset([FromBody] GenerateResetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Username) && string.IsNullOrWhiteSpace(request?.Email))
                return this.ErrorResponse("Username or email is required", 400);

            try
            {
                var token = await _auth.GenerateResetTokenAsync(request.Username, request.Email);
                if (string.IsNullOrWhiteSpace(token))
                    return this.ErrorResponse("User not found", 404);

                // NOTE: In production you should email the token. Returning it here for convenience/testing.
                return this.OkResponse(new { resetToken = token }, "Reset token generated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generate reset token error");
                return this.ErrorResponse("An unexpected error occurred", 500);
            }
        }

        public class ResetPasswordRequest { public string Token { get; set; } = string.Empty; public string NewPassword { get; set; } = string.Empty; }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Token) || string.IsNullOrWhiteSpace(request?.NewPassword))
                return this.ErrorResponse("Token and new password are required", 400);

            try
            {
                var ok = await _auth.ResetPasswordAsync(request.Token, request.NewPassword);
                if (!ok) return this.ErrorResponse("Invalid or expired token", 400);
                return this.OkResponse<object>(null, "Password has been reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password error");
                return this.ErrorResponse("An unexpected error occurred", 500);
            }
        }

        /// <summary>
        /// Get current user information including vendor details.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new
                {
                    userId = user.UserId,
                    username = user.Username,
                    email = user.Email,
                    vendorId = user.VendorId,
                    vendorName = user.VendorName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return this.ErrorResponse("An unexpected error occurred", 500);
            }
        }
    }
}
