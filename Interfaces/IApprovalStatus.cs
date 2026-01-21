using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IApprovalStatus
    {
        Task<IEnumerable<ApprovalStatusDto>> GetAllAsync();
        Task<ApprovalStatusDto?> GetByIdAsync(int id);
    }
}
