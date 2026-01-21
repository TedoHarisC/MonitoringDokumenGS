using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class Contract
    {
        [Key]
        public Guid ContractId { get; set; }
        public Guid VendorId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string ContractNumber { get; set; } = default!;
        public string ContractDescription { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ApprovalStatusId { get; set; }
        public int ContractStatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public Vendor Vendor { get; set; } = default!;
        public UserModel Creator { get; set; } = default!;
        public ApprovalStatus ApprovalStatus { get; set; } = default!;
        public ContractStatus ContractStatus { get; set; } = default!;
    }
}