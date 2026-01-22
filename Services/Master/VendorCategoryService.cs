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
    public class VendorCategoryService : IVendorCategory
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;

        public VendorCategoryService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<VendorCategoryDto>> GetAllAsync()
        {
            return await _context.VendorCategories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(VendorCategoryMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<VendorCategoryDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.VendorCategories
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(VendorCategoryMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<VendorCategoryDto?> GetByIdAsync(int id)
        {
            return await _context.VendorCategories
                .AsNoTracking()
                .Where(x => x.VendorCategoryId == id && !x.IsDeleted)
                .Select(VendorCategoryMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<VendorCategoryDto> CreateAsync(VendorCategoryDto dto)
        {
            var entity = new VendorCategory
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
            };

            _context.VendorCategories.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync(
                "VendorCategory",
                "Create",
                null,
                result,
                entity.VendorCategoryId.ToString()
            );

            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(VendorCategoryDto dto)
        {
            var entity = await _context.VendorCategories
                .FirstOrDefaultAsync(x => x.VendorCategoryId == dto.VendorCategoryId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.Name = dto.Name;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "VendorCategory",
                "Update",
                old,
                entity.ToDto(),
                entity.VendorCategoryId.ToString()
            );

            return true;
        }

        // ========================= DELETE (SOFT) =========================
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.VendorCategories
                .FirstOrDefaultAsync(x => x.VendorCategoryId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            // entity.IsDeleted = true;
            // entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync(
                "VendorCategory",
                "Delete",
                old,
                null,
                id.ToString()
            );

            return true;
        }
    }
}
