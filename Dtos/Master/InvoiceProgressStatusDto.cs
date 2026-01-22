namespace MonitoringDokumenGS.Dtos.Master
{
    public class InvoiceProgressStatusDto : SoftDeletableEntity
    {
        public int ProgressStatusId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
