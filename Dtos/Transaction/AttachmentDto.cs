using System;

namespace MonitoringDokumenGS.Dtos.Transaction
{
    public class AttachmentDto
    {
        public Guid AttachmentId { get; set; }
        public int AttachmentTypeId { get; set; }
        public Guid ReferenceId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
