using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class VendorPics : SoftDeletableEntity
    {
        [Key]
        public Guid VendorPicId { get; set; }
        public Guid VendorId { get; set; }
        public string? PicName { get; set; }
        public string? PicNumber { get; set; }
        public string? PicEmail { get; set; }
    }
}