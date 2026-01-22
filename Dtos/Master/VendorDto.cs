using System;

namespace MonitoringDokumenGS.Dtos.Master
{
    public class VendorDto : SoftDeletableEntity
    {
        public Guid VendorId { get; set; }
        public string VendorCode { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public int VendorCategoryId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string NPWP { get; set; } = string.Empty;
    }
}
