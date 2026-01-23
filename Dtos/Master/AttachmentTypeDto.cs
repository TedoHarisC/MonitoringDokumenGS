using System;

namespace MonitoringDokumenGS.Dtos.Master
{
    public class AttachmentTypeDto : SoftDeletableEntity
    {
        public int AttachmentTypeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string AppliesTo { get; set; } = string.Empty;
    }
}
