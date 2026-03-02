using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IBudgetCode
    {
        Task<IEnumerable<BudgetCodeDto>> GetAllAsync();
        Task<PagedResponse<BudgetCodeDto>> GetPagedAsync(int page, int pageSize);
        Task<BudgetCodeDto?> GetByIdAsync(Guid id);
        Task<BudgetCodeDto> CreateAsync(BudgetCodeDto dto);
        Task<bool> UpdateAsync(BudgetCodeDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
