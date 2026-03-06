using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Mappings.Master;

namespace MonitoringDokumenGS.Services.Master
{
    public class BudgetCodeService : IBudgetCode
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;

        public BudgetCodeService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<BudgetCodeDto>> GetAllAsync()
        {
            return await _context.BudgetCode
                .AsNoTracking()
                .OrderBy(x => x.Code)
                .Select(BudgetCodeMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<BudgetCodeDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.BudgetCode
                .AsNoTracking()
                .OrderBy(x => x.Code)
                .Select(BudgetCodeMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<BudgetCodeDto?> GetByIdAsync(Guid id)
        {
            return await _context.BudgetCode
                .AsNoTracking()
                .Where(x => x.BudgetCodeId == id)
                .Select(BudgetCodeMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<BudgetCodeDto> CreateAsync(BudgetCodeDto dto)
        {
            var descExists = await _context.BudgetCode
                .AnyAsync(x => x.Description.ToLower() == dto.Description.ToLower().Trim());
            if (descExists)
                throw new InvalidOperationException($"Description '{dto.Description}' sudah terdaftar.");

            var entity = new BudgetCode
            {
                BudgetCodeId = Guid.NewGuid(),
                Code = dto.Code,
                Description = dto.Description,
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow,
            };

            _context.BudgetCode.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync(
                "BudgetCode",
                "Create",
                null,
                result,
                entity.BudgetCodeId.ToString()
            );

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(BudgetCodeDto dto)
        {
            var entity = await _context.BudgetCode
                .FirstOrDefaultAsync(x => x.BudgetCodeId == dto.BudgetCodeId);

            if (entity == null)
                return false;

            var descExists = await _context.BudgetCode
                .AnyAsync(x => x.Description.ToLower() == dto.Description.ToLower().Trim() && x.BudgetCodeId != dto.BudgetCodeId);
            if (descExists)
                throw new InvalidOperationException($"Description '{dto.Description}' sudah terdaftar.");

            var old = entity.ToDto();

            entity.Code = dto.Code;
            entity.Description = dto.Description;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "BudgetCode",
                "Update",
                old,
                entity.ToDto(),
                entity.BudgetCodeId.ToString()
            );

            return true;
        }

        // ========================= DELETE =========================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.BudgetCode
                .FirstOrDefaultAsync(x => x.BudgetCodeId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            _context.BudgetCode.Remove(entity);
            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "BudgetCode",
                "Delete",
                old,
                null,
                entity.BudgetCodeId.ToString()
            );

            return true;
        }
    }
}
