namespace MonitoringDokumenGS.Dtos
{
    // Untuk Expiring Contracts notification email
    public class ExpiringContractDto
    {
        public Guid ContractId { get; set; }
        public string ContractNo { get; set; } = string.Empty;
        public Guid VendorId { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysLeft { get; set; }
        public string PicEmail { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "id";

    }
}