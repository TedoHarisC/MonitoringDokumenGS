using System;

namespace MonitoringDokumenGS.Dtos.Master
{
    public class BudgetCodeDto
    {
        public Guid BudgetCodeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
