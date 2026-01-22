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
    public class InvoiceProgressStatusService : IInvoiceProgressStatuses
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;

        public InvoiceProgressStatusService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<InvoiceProgressStatusDto>> GetAllAsync()
        {
            return await _context.InvoiceProgressStatuses
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(InvoiceProgressStatusMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<InvoiceProgressStatusDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.InvoiceProgressStatuses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(InvoiceProgressStatusMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<InvoiceProgressStatusDto?> GetByIdAsync(int id)
        {
            return await _context.InvoiceProgressStatuses
                .AsNoTracking()
                .Where(x => x.ProgressStatusId == id && !x.IsDeleted)
                .Select(InvoiceProgressStatusMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<InvoiceProgressStatusDto> CreateAsync(InvoiceProgressStatusDto dto)
        {
            var entity = new InvoiceProgressStatuses
            {
                Code = dto.Code,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
                IsDeleted = false
            };

            _context.InvoiceProgressStatuses.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync(
                "InvoiceProgressStatus",
                "Create",
                null,
                result,
                entity.ProgressStatusId.ToString()
            );

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(InvoiceProgressStatusDto dto)
        {
            var entity = await _context.InvoiceProgressStatuses
                .FirstOrDefaultAsync(x => x.ProgressStatusId == dto.ProgressStatusId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = dto.UpdatedBy;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "InvoiceProgressStatus",
                "Update",
                old,
                entity.ToDto(),
                entity.ProgressStatusId.ToString()
            );

            return true;
        }

        // ========================= DELETE (SOFT) =========================
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.InvoiceProgressStatuses
                .FirstOrDefaultAsync(x => x.ProgressStatusId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "InvoiceProgressStatus",
                "Delete",
                old,
                null,
                id.ToString()
            );

            return true;
        }
    }
}
