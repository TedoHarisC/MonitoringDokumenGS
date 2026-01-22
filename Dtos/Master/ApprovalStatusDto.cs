namespace MonitoringDokumenGS.Dtos.Master
{
    public class ApprovalStatusDto : SoftDeletableEntity
    {
        public int ApprovalStatusId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
