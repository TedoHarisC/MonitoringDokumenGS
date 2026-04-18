using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitoringDokumenGS.Models.Transaction
{
    public class UangMuka
    {
        [Key]
        public string UangMukaId { get; set; } = string.Empty;
        public required string Jenis { get; set; }
        public required Guid BudgetCodeId { get; set; }
        public required int CoaTextId { get; set; }
        public string? UangMukaRelatedId { get; set; }
        public string? NoSAP { get; set; }
        public decimal Amount { get; set; }
        public required string AtasNama { get; set; }
        public string Deskripsi { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public required string StatusId { get; set; }
        [NotMapped]
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        [ForeignKey("BudgetCodeId")]
        public BudgetCode? BudgetCode { get; set; }
        [ForeignKey("UangMukaRelatedId")]
        public UangMuka? RelatedUangMuka { get; set; }
        [ForeignKey("CoaTextId")]
        public VendorCategory? CoaText { get; set; }
    }
}