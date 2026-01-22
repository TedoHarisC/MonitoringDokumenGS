using System;

namespace MonitoringDokumenGS.Dtos.Master
{
    public class VendorCategoryDto : SoftDeletableEntity
    {
        public int VendorCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
