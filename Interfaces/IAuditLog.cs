using MonitoringDokumenGS.Dtos.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IAuditLog
    {
        Task<IEnumerable<AuditLogDto>> GetAllAsync();
        Task<AuditLogDto?> GetByIdAsync(Guid id);
        Task<AuditLogDto> CreateAsync(AuditLogDto dto);
    }
}
