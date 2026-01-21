using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IVendor
    {
        Task<IEnumerable<VendorDto>> GetAllAsync();
        Task<PagedResponse<VendorDto>> GetPagedAsync(int page, int pageSize);
        Task<VendorDto?> GetByIdAsync(Guid id);
        Task<VendorDto> CreateAsync(VendorDto dto);
        Task<bool> UpdateAsync(VendorDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
