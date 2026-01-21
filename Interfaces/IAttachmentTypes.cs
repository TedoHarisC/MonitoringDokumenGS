using MonitoringDokumenGS.Dtos.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IAttachmentTypes
    {
        Task<IEnumerable<AttachmentTypeDto>> GetAllAsync();
        Task<AttachmentTypeDto?> GetByIdAsync(int id);
        Task<AttachmentTypeDto> CreateAsync(AttachmentTypeDto dto);
    }
}
