using System;

namespace MonitoringDokumenGS.Dtos.Auth
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public Guid VendorId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
