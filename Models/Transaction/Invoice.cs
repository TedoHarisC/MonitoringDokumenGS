using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class Invoice
    {
        [Key]
        public Guid InvoiceId { get; set; }
        public Guid VendorId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public int ProgressStatusId { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int InvoiceYear { get; set; }
        public int InvoiceMonth { get; set; }

        // Navigation properties
        public Vendor Vendor { get; set; } = default!;
        public UserModel Creator { get; set; } = default!;
        public InvoiceProgressStatuses ProgressStatus { get; set; } = default!;
    }
}