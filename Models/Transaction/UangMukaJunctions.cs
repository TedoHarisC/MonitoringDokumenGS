using System;
using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models.Transaction
{
    public class UangMukaBudgetCode
    {
        [Key]
        public int Id { get; set; } // Primary key for junction table
        public string UangMukaId { get; set; } = string.Empty;
        public Guid BudgetCodeId { get; set; }
        // Navigation properties (optional, not required if no FK constraint)
        public UangMuka? UangMuka { get; set; }
        public BudgetCode? BudgetCode { get; set; }
    }

    public class UangMukaCoaText
    {
        [Key]
        public int Id { get; set; } // Primary key for junction table
        public string UangMukaId { get; set; } = string.Empty;
        public int CoaTextId { get; set; }
        // Navigation properties (optional)
        public UangMuka? UangMuka { get; set; }
        public VendorCategory? CoaText { get; set; }
    }
}
