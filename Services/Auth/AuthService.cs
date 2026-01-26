using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dtos.Login;
using Dtos.Register;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models;
using MonitoringDokumenGS.Services.Infrastructure;
using BCryptNet = BCrypt.Net.BCrypt;

namespace MonitoringDokumenGS.Services
{
    public class AuthService : IAuth
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;

        public AuthService(
            ApplicationDBContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto registerDto)
        {
            if (registerDto is null) throw new ArgumentNullException(nameof(registerDto));

            // Check if username or email already exists (excluding soft-deleted users)
            var exists = await _context.Users
                .AnyAsync(u => (u.Username == registerDto.Username || u.Email == registerDto.Email) && !u.isDeleted);
            if (exists)
                throw new InvalidOperationException("Username or email already exists");

            // VALIDASI FK
            var vendorExists = await _context.Vendors
                .AnyAsync(v => v.VendorId == registerDto.VendorId);

            if (!vendorExists)
                throw new Exception("Vendor not found");

            // Create Id
            var newUserId = Guid.NewGuid();

            var user = new UserModel
            {
                UserId = newUserId,
                VendorId = registerDto.VendorId ?? Guid.Empty,
                Username = registerDto.Username,
                PasswordHash = BCryptNet.HashPassword(registerDto.Password),
                Email = registerDto.Email,
                CreatedBy = newUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                isActive = true,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(string token, string refreshToken)> LoginAsync(LoginRequestDto loginDto)
        {
            if (loginDto is null) throw new ArgumentNullException(nameof(loginDto));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null)
            {
                _logger.LogWarning("Login failed for unknown user {Username}", loginDto.Username);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (!BCryptNet.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user {Username}", loginDto.Username);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var jwt = GenerateJwtToken(user);
            var refreshToken = CreateRandomToken();

            var stored = new UserRefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = user.UserId,
                TokenHash = BCryptNet.HashPassword(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.UserRefreshTokens.Add(stored);
            await _context.SaveChangesAsync();

            return (jwt, refreshToken);
        }

        public async Task<(string token, string refreshToken)> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return (string.Empty, string.Empty);

            var candidates = await _context.UserRefreshTokens
                .Where(t => t.ExpiresAt > DateTime.UtcNow && t.RevokedAt == null)
                .ToListAsync();

            var tokenRecord = candidates
                .FirstOrDefault(t => BCryptNet.Verify(refreshToken, t.TokenHash));

            if (tokenRecord == null)
                return (string.Empty, string.Empty);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == tokenRecord.UserId);
            if (user == null) return (string.Empty, string.Empty);

            var newJwt = GenerateJwtToken(user);
            var newRefresh = CreateRandomToken();

            tokenRecord.TokenHash = BCryptNet.HashPassword(newRefresh);
            tokenRecord.ExpiresAt = DateTime.UtcNow.AddDays(7);
            tokenRecord.CreatedAt = DateTime.UtcNow;

            _context.UserRefreshTokens.Update(tokenRecord);
            await _context.SaveChangesAsync();

            return (newJwt, newRefresh);
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)) return false;

            var candidates = await _context.UserRefreshTokens
                .Where(t => t.RevokedAt == null)
                .ToListAsync();

            var tokenRecord = candidates
                .FirstOrDefault(t => BCryptNet.Verify(refreshToken, t.TokenHash));

            if (tokenRecord == null) return false;

            tokenRecord.RevokedAt = DateTime.UtcNow;
            _context.UserRefreshTokens.Update(tokenRecord);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateResetTokenAsync(string username, string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
            if (user == null) return string.Empty;

            var resetToken = CreateRandomToken();
            var record = new UserRefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = user.UserId,
                TokenHash = BCryptNet.HashPassword(resetToken),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow
            };

            _context.UserRefreshTokens.Add(record);
            await _context.SaveChangesAsync();

            // Send password reset email
            try
            {
                var resetLink = $"{_configuration["AppUrl"] ?? "http://localhost:5170"}/Auth/ResetPassword?token={resetToken}";
                var htmlBody = EmailTemplateHelper.GetPasswordResetEmail(
                    userName: user.Username ?? "User",
                    resetLink: resetLink,
                    expirationTime: "1 hour"
                );

                await _emailService.SendAsync(
                    to: user.Email ?? email,
                    subject: "Password Reset Request - ABB Monitoring System",
                    htmlBody: htmlBody
                );

                _logger.LogInformation("Password reset email sent to {Email}", user.Email ?? email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email ?? email);
                // Don't fail the request if email fails, token is still valid
            }

            return resetToken;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword)) return false;

            var candidates = await _context.UserRefreshTokens
                .Where(t => t.ExpiresAt > DateTime.UtcNow && t.RevokedAt == null)
                .ToListAsync();

            // Verify di Memori
            var tokenRecord = candidates
                .FirstOrDefault(t => BCryptNet.Verify(token, t.TokenHash));

            if (tokenRecord == null) return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == tokenRecord.UserId);
            if (user == null) return false;

            user.PasswordHash = BCryptNet.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            tokenRecord.RevokedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            _context.UserRefreshTokens.Update(tokenRecord);
            await _context.SaveChangesAsync();

            return true;
        }

        // Helper methods
        private string GenerateJwtToken(UserModel user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expiresInHours = 24;
            if (!int.TryParse(_configuration["Jwt:ExpiresHours"], out expiresInHours))
                expiresInHours = 24;

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("Jwt:Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("username", user.Username ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiresInHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string CreateRandomToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
    }
}
