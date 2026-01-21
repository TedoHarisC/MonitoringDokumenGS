using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IContractStatus
    {
        Task<IEnumerable<ContractStatusDto>> GetAllAsync();
        Task<ContractStatusDto?> GetByIdAsync(int id);
    }
}
