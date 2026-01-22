namespace MonitoringDokumenGS.Dtos.Master
{
    public class ContractStatusDto : SoftDeletableEntity
    {
        public int ContractStatusId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
