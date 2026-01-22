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
    public class ContractStatusService : IContractStatus
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;

        public ContractStatusService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<ContractStatusDto>> GetAllAsync()
        {
            return await _context.ContractStatuses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(ContractStatusMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<ContractStatusDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.ContractStatuses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(ContractStatusMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<ContractStatusDto?> GetByIdAsync(int id)
        {
            return await _context.ContractStatuses
                .AsNoTracking()
                .Where(x => x.ContractStatusId == id)
                .Select(ContractStatusMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<ContractStatusDto> CreateAsync(ContractStatusDto dto)
        {
            var entity = new ContractStatus
            {
                Code = dto.Code,
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
                IsDeleted = false
            };

            _context.ContractStatuses.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync(
                "ContractStatus",
                "Create",
                null,
                result,
                entity.ContractStatusId.ToString()
            );

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(ContractStatusDto dto)
        {
            var entity = await _context.ContractStatuses
                .FirstOrDefaultAsync(x => x.ContractStatusId == dto.ContractStatusId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = dto.UpdatedBy;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "ContractStatus",
                "Update",
                old,
                entity.ToDto(),
                entity.ContractStatusId.ToString()
            );

            return true;
        }

        // ========================= DELETE (SOFT) =========================
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ContractStatuses
                .FirstOrDefaultAsync(x => x.ContractStatusId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "ContractStatus",
                "Delete",
                old,
                null,
                id.ToString()
            );

            return true;
        }
    }
}
