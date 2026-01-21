using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IAttachmentTypes
    {
        Task<IEnumerable<AttachmentTypeDto>> GetAllAsync();
        Task<PagedResponse<AttachmentTypeDto>> GetPagedAsync(int page, int pageSize);
        Task<AttachmentTypeDto?> GetByIdAsync(int id);
        Task<AttachmentTypeDto> CreateAsync(AttachmentTypeDto dto);
        Task<bool> UpdateAsync(AttachmentTypeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
