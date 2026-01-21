using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class VendorCategory
    {
        [Key]
        public int VendorCategoryId { get; set; }
        public string Name { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}