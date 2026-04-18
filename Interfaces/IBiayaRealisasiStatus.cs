using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Dtos.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IBiayaRealisasiStatus
    {
        Task<IEnumerable<BiayaRealisasiStatusDto>> GetAllAsync();
        Task<PagedResponse<BiayaRealisasiStatusDto>> GetPagedAsync(int page, int pageSize);
        Task<BiayaRealisasiStatusDto?> GetByIdAsync(string id);
        Task<BiayaRealisasiStatusDto> CreateAsync(BiayaRealisasiStatusDto dto);
        Task<bool> UpdateAsync(BiayaRealisasiStatusDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
