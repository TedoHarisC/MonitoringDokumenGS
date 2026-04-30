
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IVendorCategory
    {
        Task<IEnumerable<VendorCategoryDto>> GetAllAsync();
        Task<PagedResponse<VendorCategoryDto>> GetPagedAsync(int page, int pageSize);
        Task<VendorCategoryDto?> GetByIdAsync(int id);
        // For multiple Budget Code IDs
        Task<IEnumerable<VendorCategoryDto>> GetByBudgetCodeIdsAsync(IEnumerable<Guid> budgetCodeIds);
        Task<VendorCategoryDto> CreateAsync(VendorCategoryDto dto);
        Task<bool> UpdateAsync(VendorCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
