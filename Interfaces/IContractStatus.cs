using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IContractStatus
    {
        Task<IEnumerable<ContractStatusDto>> GetAllAsync();
        Task<PagedResponse<ContractStatusDto>> GetPagedAsync(int page, int pageSize);
        Task<ContractStatusDto?> GetByIdAsync(int id);
        Task<ContractStatusDto> CreateAsync(ContractStatusDto dto);
        Task<bool> UpdateAsync(ContractStatusDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
