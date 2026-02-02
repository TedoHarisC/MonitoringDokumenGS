namespace MonitoringDokumenGS.Dtos.Dashboard
{
    public class ContractExpiringDto
    {
        public Guid ContractId { get; set; }
        public string ContractNo { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysRemaining { get; set; }
        public decimal ContractValue { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AlertLevel { get; set; } = string.Empty; // Critical, Warning, Safe
        public string Description { get; set; } = string.Empty;
    }
}
