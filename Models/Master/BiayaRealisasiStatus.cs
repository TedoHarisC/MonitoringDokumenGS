using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models.Master
{
    public class BiayaRealisasiStatus : SoftDeletableEntity
    {
        [Key]
        public required string BiayaRealisasiStatusesId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}