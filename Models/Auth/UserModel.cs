using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class UserModel
    {
        [Key]
        public Guid UserId { get; set; }
        public Guid VendorId { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Email { get; set; }
        public bool isActive { get; set; } = true;
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool isDeleted { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public Guid SecurityStamp { get; set; } = Guid.NewGuid();
        // Navigation Properties
        public virtual Vendor? Vendor { get; set; }
    }
}