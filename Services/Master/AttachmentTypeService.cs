using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Mappings.Master;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Services.Master
{
    public class AttachmentTypeService : IAttachmentTypes
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;

        public AttachmentTypeService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<AttachmentTypeDto>> GetAllAsync()
        {
            return await _context.AttachmentTypes
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(AttachmentTypeMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<AttachmentTypeDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.AttachmentTypes
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(AttachmentTypeMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<AttachmentTypeDto?> GetByIdAsync(int id)
        {
            return await _context.AttachmentTypes
                .Where(x => x.AttachmentTypeId == id && !x.IsDeleted)
                .Select(AttachmentTypeMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<AttachmentTypeDto> CreateAsync(AttachmentTypeDto dto)
        {
            var entity = new AttachmentTypes
            {
                Code = dto.Code,
                Name = dto.Name,
                IsActive = dto.IsActive,
                AppliesTo = dto.AppliesTo,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
                IsDeleted = false
            };

            _context.AttachmentTypes.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync(
                "AttachmentType",
                "Create",
                null,
                result,
                entity.AttachmentTypeId.ToString()
            );

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(AttachmentTypeDto dto)
        {
            var entity = await _context.AttachmentTypes
                .FirstOrDefaultAsync(x => x.AttachmentTypeId == dto.AttachmentTypeId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.IsActive = dto.IsActive;
            entity.AppliesTo = dto.AppliesTo;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = dto.UpdatedBy;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "AttachmentType",
                "Update",
                old,
                entity.ToDto(),
                entity.AttachmentTypeId.ToString()
            );

            return true;
        }

        // ========================= DELETE (SOFT) =========================
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.AttachmentTypes
                .FirstOrDefaultAsync(x => x.AttachmentTypeId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "AttachmentType",
                "Delete",
                old,
                null,
                id.ToString()
            );

            return true;
        }
    }
}
