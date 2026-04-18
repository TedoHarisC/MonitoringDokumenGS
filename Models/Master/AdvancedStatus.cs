using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models.Master
{
    public class AdvancedStatus : SoftDeletableEntity
    {
        [Key]
        public required string AdvancedStatusesId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}