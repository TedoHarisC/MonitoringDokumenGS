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
    public class ApprovalStatusService : IApprovalStatus
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;
        public ApprovalStatusService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<ApprovalStatusDto>> GetAllAsync()
        {
            return await _context.ApprovalStatuses
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ApprovalStatusId)
                .Select(ApprovalStatusMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<ApprovalStatusDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.ApprovalStatuses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ApprovalStatusId)
                .Select(ApprovalStatusMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<ApprovalStatusDto?> GetByIdAsync(int id)
        {
            return await _context.ApprovalStatuses
                .AsNoTracking()
                .Where(x => x.ApprovalStatusId == id && !x.IsDeleted)
                .Select(ApprovalStatusMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<ApprovalStatusDto> CreateAsync(ApprovalStatusDto dto)
        {
            var entity = new ApprovalStatus
            {
                Code = dto.Code,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
                IsDeleted = false
            };

            _context.ApprovalStatuses.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync(
                "ApprovalStatus",
                "Create",
                null,
                result,
                entity.ApprovalStatusId.ToString()
            );

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(ApprovalStatusDto dto)
        {
            var entity = await _context.ApprovalStatuses
                .FirstOrDefaultAsync(x => x.ApprovalStatusId == dto.ApprovalStatusId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = dto.UpdatedBy;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "ApprovalStatus",
                "Update",
                old,
                entity.ToDto(),
                entity.ApprovalStatusId.ToString()
            );

            return true;
        }

        // ========================= DELETE (SOFT) =========================
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ApprovalStatuses
                .FirstOrDefaultAsync(x => x.ApprovalStatusId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "ApprovalStatus",
                "Delete",
                old,
                null,
                id.ToString()
            );

            return true;
        }
    }
}
