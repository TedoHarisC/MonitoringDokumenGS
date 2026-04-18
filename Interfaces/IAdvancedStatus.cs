using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Dtos.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IAdvancedStatus
    {
        Task<IEnumerable<AdvancedStatusDto>> GetAllAsync();
        Task<PagedResponse<AdvancedStatusDto>> GetPagedAsync(int page, int pageSize);
        Task<AdvancedStatusDto?> GetByIdAsync(string id);
        Task<AdvancedStatusDto> CreateAsync(AdvancedStatusDto dto);
        Task<bool> UpdateAsync(AdvancedStatusDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
