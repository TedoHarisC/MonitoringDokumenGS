using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class VendorCategory : SoftDeletableEntity
    {
        [Key]
        public int VendorCategoryId { get; set; }
        public string Name { get; set; } = default!;
    }
}