using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class InvoiceProgressStatuses
    {
        [Key]
        public int ProgressStatusId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}