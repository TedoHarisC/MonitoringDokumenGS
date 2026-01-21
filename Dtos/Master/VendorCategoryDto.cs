using System;

namespace MonitoringDokumenGS.Dtos.Master
{
    public class VendorCategoryDto
    {
        public int VendorCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
