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

    public class AuditHistoryDto
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string StatusTransition { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Jenis { get; set; } = string.Empty; // Khusus untuk Uang Muka
        // Timeline
        public string DisplayText => $"{StatusTransition} oleh {Username} pada {CreatedAt:dd MMMM yyyy HH:mm}";
    }
}
