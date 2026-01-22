using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class VendorCategory
    {
        [Key]
        public int VendorCategoryId { get; set; }
        public string Name { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public UserModel Users { get; set; } = default!;
    }
}