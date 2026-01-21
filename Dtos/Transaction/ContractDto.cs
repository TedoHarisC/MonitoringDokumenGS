using System;

namespace MonitoringDokumenGS.Dtos.Transaction
{
    public class ContractDto
    {
        public Guid ContractId { get; set; }
        public Guid VendorId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ContractDescription { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ApprovalStatusId { get; set; }
        public int ContractStatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
