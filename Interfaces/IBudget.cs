using MonitoringDokumenGS.Dtos;
using MonitoringDokumenGS.Dtos.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IBudget
    {
        Task<IEnumerable<BudgetDto>> GetAllAsync();
        Task<PagedResponse<BudgetDto>> GetPagedAsync(int page, int pageSize);
        Task<BudgetDto?> GetByIdAsync(Guid id);
        Task<BudgetDto> CreateAsync(BudgetDto dto);
        Task<bool> UpdateAsync(BudgetDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
