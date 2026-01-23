using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class AuditLog
    {
        [Key]
        public Guid AuditLogId { get; set; }
        public Guid UserId { get; set; }
        public string EntityName { get; set; } = default!;
        public string EntityId { get; set; } = default!;
        public string OldData { get; set; } = default!;
        public string NewData { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}