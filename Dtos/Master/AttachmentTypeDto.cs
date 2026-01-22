using System;

namespace MonitoringDokumenGS.Dtos.Master
{
    public class AttachmentTypeDto
    {
        public int AttachmentTypeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string AppliesTo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; } = Guid.Empty;
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; } = Guid.Empty;
        public bool IsDeleted { get; set; }
    }
}
