using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class InvoiceProgressStatuses
    {
        [Key]
        public int ProgressStatusId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}