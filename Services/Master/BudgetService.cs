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
                .Select(BudgetMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<BudgetDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.MST_Budget
                .OrderByDescending(x => x.Year)
                .Select(BudgetMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<BudgetDto?> GetByIdAsync(Guid id)
        {
            return await _context.MST_Budget
                .Where(x => x.BudgetId == id)
                .Select(BudgetMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<BudgetDto> CreateAsync(BudgetDto dto)
        {
            var entity = new Budget
            {
                BudgetId = Guid.NewGuid(),
                Year = dto.Year,
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
