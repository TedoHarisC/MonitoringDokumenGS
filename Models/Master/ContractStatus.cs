namespace MonitoringDokumenGS.Models
{
    public class ContractStatus : SoftDeletableEntity
    {
        public int ContractStatusId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}