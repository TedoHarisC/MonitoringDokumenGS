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
    public class BiayaRealisasiStatusService : IBiayaRealisasiStatus
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;
        public BiayaRealisasiStatusService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        public async Task<IEnumerable<BiayaRealisasiStatusDto>> GetAllAsync()
        {
            return await _context.BiayaRealisasiStatuses
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.BiayaRealisasiStatusesId)
                .Select(BiayaRealisasiStatusMappings.ToDtoExpr)
                .ToListAsync();
        }

        public async Task<PagedResponse<BiayaRealisasiStatusDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.BiayaRealisasiStatuses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.BiayaRealisasiStatusesId)
                .Select(BiayaRealisasiStatusMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        public async Task<BiayaRealisasiStatusDto?> GetByIdAsync(string id)
        {
            return await _context.BiayaRealisasiStatuses
                .AsNoTracking()
                .Where(x => x.BiayaRealisasiStatusesId == id && !x.IsDeleted)
                .Select(BiayaRealisasiStatusMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        public async Task<BiayaRealisasiStatusDto> CreateAsync(BiayaRealisasiStatusDto dto)
        {
            var entity = new BiayaRealisasiStatus
            {
                BiayaRealisasiStatusesId = Guid.NewGuid().ToString(),
                Code = dto.Code,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
            };
            _context.BiayaRealisasiStatuses.Add(entity);
            await _context.SaveChangesAsync();
            return BiayaRealisasiStatusMappings.ToDto(entity);
        }

        public async Task<bool> UpdateAsync(BiayaRealisasiStatusDto dto)
        {
            var entity = await _context.BiayaRealisasiStatuses.FirstOrDefaultAsync(x => x.BiayaRealisasiStatusesId == dto.BiayaRealisasiStatusId && !x.IsDeleted);
            if (entity == null) return false;
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.BiayaRealisasiStatuses.FirstOrDefaultAsync(x => x.BiayaRealisasiStatusesId == id && !x.IsDeleted);
            if (entity == null) return false;
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
