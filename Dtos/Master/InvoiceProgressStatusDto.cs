namespace MonitoringDokumenGS.Dtos.Master
{
    public class InvoiceProgressStatusDto
    {
        public int ProgressStatusId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; } = Guid.Empty;
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; } = Guid.Empty;
        public bool IsDeleted { get; set; }
    }
}
