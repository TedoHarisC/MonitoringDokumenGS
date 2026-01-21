using Dtos.Login;
using Dtos.Register;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IAuth
    {
        Task<bool> RegisterAsync(RegisterRequestDto registerDto);
        Task<(string token, string refreshToken)> LoginAsync(LoginRequestDto loginDto);
        Task<(string token, string refreshToken)> RefreshTokenAsync(string refreshToken);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<string> GenerateResetTokenAsync(string username, string email);
        Task<bool> LogoutAsync(string refreshToken);
    }
}