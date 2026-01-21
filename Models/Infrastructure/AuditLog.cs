using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class AuditLog
    {
        [Key]
        public Guid AuditLogId { get; set; }
        public Guid UserId { get; set; }
        public string EntityName { get; set; } = default!;
        public Guid EntityId { get; set; }
        public string OldData { get; set; } = default!;
        public string NewData { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public UserModel User { get; set; } = default!;
        public Vendor Vendor { get; set; } = default!;
    }
}