using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Mappings.Master;
using MonitoringDokumenGS.Models.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Services.Master
{
    public class AdvancedStatusService : IAdvancedStatus
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;
        public AdvancedStatusService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<IEnumerable<AdvancedStatusDto>> GetAllAsync()
        {
            return await _context.AdvancedStatuses
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.AdvancedStatusesId)
                .Select(AdvancedStatusMappings.ToDtoExpr)
                .ToListAsync();
        }

        public async Task<PagedResponse<AdvancedStatusDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.AdvancedStatuses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.AdvancedStatusesId)
                .Select(AdvancedStatusMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        public async Task<AdvancedStatusDto?> GetByIdAsync(string id)
        {
            return await _context.AdvancedStatuses
                .AsNoTracking()
                .Where(x => x.AdvancedStatusesId == id && !x.IsDeleted)
                .Select(AdvancedStatusMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        public async Task<AdvancedStatusDto> CreateAsync(AdvancedStatusDto dto)
        {
            var entity = new AdvancedStatus
            {
                AdvancedStatusesId = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
            };
            _context.AdvancedStatuses.Add(entity);
            await _context.SaveChangesAsync();
            return AdvancedStatusMappings.ToDto(entity);
        }

        public async Task<bool> UpdateAsync(AdvancedStatusDto dto)
        {
            var entity = await _context.AdvancedStatuses.FirstOrDefaultAsync(x => x.AdvancedStatusesId == dto.AdvancedStatusId && !x.IsDeleted);
            if (entity == null) return false;
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.AdvancedStatuses.FirstOrDefaultAsync(x => x.AdvancedStatusesId == id && !x.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
