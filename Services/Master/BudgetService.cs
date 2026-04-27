using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Mappings.Master;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Services.Master
{
    public class BudgetService : IBudget
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;

        public BudgetService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<BudgetDto>> GetAllAsync()
        {
            return await _context.MST_Budget
                .Select(x => new BudgetDto
                {
                    BudgetId = x.BudgetId,
                    Year = x.Year,
                    BudgetCodeId = x.BudgetCodeId,
                    BudgetCodeLabel = x.BudgetCodeId == null ? null :
                        _context.BudgetCode
                            .Where(b => b.BudgetCodeId == x.BudgetCodeId)
                            .Select(b => b.Code + " - " + b.Description)
                            .FirstOrDefault(),
                    NoCoa = x.NoCoa,
                    TypeBudget = x.TypeBudget,
                    Activity = x.Activity,
                    TotalBudget = x.TotalBudget,
                    MonthlyBudget = x.MonthlyBudget,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy
                })
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<BudgetDto>> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.MST_Budget.Include(x => x.BudgetCode).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(x =>
                    x.Year.ToString().Contains(search) ||
                    (x.NoCoa != null && x.NoCoa.ToLower().Contains(search)) ||
                    (x.TypeBudget != null && x.TypeBudget.ToLower().Contains(search)) ||
                    (x.Activity != null && x.Activity.ToLower().Contains(search)) ||
                    (x.BudgetCode != null && (
                        x.BudgetCode.Code.ToLower().Contains(search) ||
                        x.BudgetCode.Description.ToLower().Contains(search)
                    ))
                );
            }
            return await query
                .OrderByDescending(x => x.Year)
                .Select(x => new BudgetDto
                {
                    BudgetId = x.BudgetId,
                    Year = x.Year,
                    BudgetCodeId = x.BudgetCodeId,
                    BudgetCodeLabel = x.BudgetCode != null ? x.BudgetCode.Code + " - " + x.BudgetCode.Description : null,
                    NoCoa = x.NoCoa,
                    TypeBudget = x.TypeBudget,
                    Activity = x.Activity,
                    TotalBudget = x.TotalBudget,
                    MonthlyBudget = x.MonthlyBudget,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy
                })
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<BudgetDto?> GetByIdAsync(Guid id)
        {
            return await _context.MST_Budget
                .Where(x => x.BudgetId == id)
                .Select(x => new BudgetDto
                {
                    BudgetId = x.BudgetId,
                    Year = x.Year,
                    BudgetCodeId = x.BudgetCodeId,
                    BudgetCodeLabel = x.BudgetCodeId == null ? null :
                        _context.BudgetCode
                            .Where(b => b.BudgetCodeId == x.BudgetCodeId)
                            .Select(b => b.Code + " - " + b.Description)
                            .FirstOrDefault(),
                    NoCoa = x.NoCoa,
                    TypeBudget = x.TypeBudget,
                    Activity = x.Activity,
                    TotalBudget = x.TotalBudget,
                    MonthlyBudget = x.MonthlyBudget,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy
                })
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<BudgetDto> CreateAsync(BudgetDto dto)
        {
            var entity = new Budget
            {
                BudgetId = Guid.NewGuid(),
                Year = dto.Year,
                BudgetCodeId = dto.BudgetCodeId,
                NoCoa = dto.NoCoa,
                TypeBudget = dto.TypeBudget,
                Activity = dto.Activity,
                TotalBudget = dto.TotalBudget,
                MonthlyBudget = dto.MonthlyBudget,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy
            };

            _context.MST_Budget.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync("Budget", "Create", null, result, entity.BudgetId.ToString());
            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(BudgetDto dto)
        {
            var entity = await _context.MST_Budget
                .FirstOrDefaultAsync(x => x.BudgetId == dto.BudgetId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.Year = dto.Year;
            entity.BudgetCodeId = dto.BudgetCodeId;
            entity.NoCoa = dto.NoCoa;
            entity.TypeBudget = dto.TypeBudget;
            entity.Activity = dto.Activity;
            entity.TotalBudget = dto.TotalBudget;
            entity.MonthlyBudget = dto.MonthlyBudget;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync("Budget", "Update", old, entity.ToDto(), entity.BudgetId.ToString());
            return true;
        }

        // ========================= DELETE =========================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.MST_Budget
                .FirstOrDefaultAsync(x => x.BudgetId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            _context.MST_Budget.Remove(entity);
            await _context.SaveChangesAsync();

            await _auditLog.LogAsync("Budget", "Delete", old, null, id.ToString());
            return true;
        }
    }
}
