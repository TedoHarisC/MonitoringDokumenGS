using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class AttachmentTypes : SoftDeletableEntity
    {
        [Key]
        public int AttachmentTypeId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool IsRequired { get; set; }
        public string AppliesTo { get; set; } = default!;
    }
}