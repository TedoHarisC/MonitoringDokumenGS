using MonitoringDokumenGS.Dtos.Transaction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IAttachment
    {
        Task<IEnumerable<AttachmentDto>> GetAllAsync();
        Task<AttachmentDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<AttachmentDto>> GetByReferenceIdAsync(Guid referenceId);
        Task<AttachmentDto> CreateAsync(AttachmentDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
