using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IApprovalStatus
    {
        Task<IEnumerable<ApprovalStatusDto>> GetAllAsync();
        Task<PagedResponse<ApprovalStatusDto>> GetPagedAsync(int page, int pageSize);
        Task<ApprovalStatusDto?> GetByIdAsync(int id);
        Task<ApprovalStatusDto> CreateAsync(ApprovalStatusDto dto);
        Task<bool> UpdateAsync(ApprovalStatusDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
