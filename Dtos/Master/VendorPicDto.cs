using System;

namespace MonitoringDokumenGS.Dtos.Master
{
    public class VendorPicDto
    {
        public Guid VendorPicId { get; set; }
        public Guid VendorId { get; set; }
        public string? PicName { get; set; }
        public string? PicNumber { get; set; }
        public string? PicEmail { get; set; }
    }
}
