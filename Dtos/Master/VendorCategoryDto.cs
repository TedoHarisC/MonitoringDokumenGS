using System;

namespace MonitoringDokumenGS.Dtos.Master
{
    public class VendorCategoryDto
    {
        public int VendorCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; } = Guid.Empty;
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; } = Guid.Empty;
        public bool IsDeleted { get; set; }
    }
}
