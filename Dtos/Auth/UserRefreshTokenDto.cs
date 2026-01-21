using System;

namespace MonitoringDokumenGS.Dtos.Auth
{
    public class UserRefreshTokenDto
    {
        public Guid RefreshTokenId { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
