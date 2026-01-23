using System;

namespace MonitoringDokumenGS.Dtos.Infrastructure
{
    public class AuditLogDto
    {
        public Guid AuditLogId { get; set; }
        public Guid UserId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string OldData { get; set; } = string.Empty;
        public string NewData { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
