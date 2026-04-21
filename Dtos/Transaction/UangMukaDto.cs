using System;

namespace MonitoringDokumenGS.Dtos.Transaction
{
    public class UangMukaDto
    {
        public string? UangMukaId { get; set; }
        public string? Jenis { get; set; }
        public List<Guid>? BudgetCodeIds { get; set; } // Multiple
        public List<int>? CoaTextIds { get; set; } // Multiple
        public string? UangMukaRelatedId { get; set; }
        public string? NoSAP { get; set; }
        public decimal Amount { get; set; }
        public string? AtasNama { get; set; }
        public string? Deskripsi { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? StatusId { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Tambahan untuk penampilan
        public List<SimpleBudgetCodeDto>? BudgetCodes { get; set; }
        public List<SimpleCoaTextDto>? CoaTexts { get; set; }
    }

    public class SimpleBudgetCodeDto
    {
        public Guid BudgetCodeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }

    public class SimpleCoaTextDto
    {
        public int CoaTextId { get; set; }
        public string? Name { get; set; }
        public string? ParentBudgetCodeLabel { get; set; }
    }
}
