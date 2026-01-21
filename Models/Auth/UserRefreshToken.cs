using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class UserRefreshToken
    {
        [Key]
        public Guid RefreshTokenId { get; set; }
        public Guid UserId { get; set; }
        public required string TokenHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByIp { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}