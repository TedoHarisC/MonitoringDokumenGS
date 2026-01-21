using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IVendorCategory
    {
        Task<IEnumerable<VendorCategoryDto>> GetAllAsync();
        Task<VendorCategoryDto?> GetByIdAsync(int id);
        Task<VendorCategoryDto> CreateAsync(VendorCategoryDto dto);
    }
}
