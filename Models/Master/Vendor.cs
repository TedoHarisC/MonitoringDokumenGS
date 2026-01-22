using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class Vendor : SoftDeletableEntity
    {
        [Key]
        public Guid VendorId { get; set; }
        public string VendorCode { get; set; } = default!;
        public string VendorName { get; set; } = default!;
        public string ShortName { get; set; } = default!;
        public int VendorCategoryId { get; set; }
        public string OwnerName { get; set; } = default!;
        public string OwnerPhone { get; set; } = default!;
        public string CompanyEmail { get; set; } = default!;
        public string NPWP { get; set; } = default!;

        // Navigation properties
        public UserModel Users { get; set; } = default!;
        public VendorCategory VendorCategory { get; set; } = default!;
    }
}