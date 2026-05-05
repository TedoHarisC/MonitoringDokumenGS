using System;

namespace MonitoringDokumenGS.Dtos.Transaction
{
    public class InvoiceDto
    {
        public Guid InvoiceId { get; set; }
        public Guid VendorId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? InvoiceDescription { get; set; }
        public string? NoSAP { get; set; }
        public int ProgressStatusId { get; set; }
        public string? ProgressStatusName { get; set; } // Nama status progress
        public string? VendorName { get; set; } // Nama vendor
        public decimal InvoiceAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int InvoiceYear { get; set; }
        public int InvoiceMonth { get; set; }
        public bool IsOnTime { get; set; }
        public Guid? BudgetCodeId { get; set; }
        public string? BudgetCode { get; set; }
        public int? CoaTextId { get; set; }
        public string? CoaText { get; set; }
    }
}
