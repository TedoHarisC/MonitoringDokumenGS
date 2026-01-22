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
    public class VendorService : IVendor
    {
        private readonly ApplicationDBContext _context;
        private readonly IAuditLog _auditLog;

        public VendorService(ApplicationDBContext context, IAuditLog auditLog)
        {
            _context = context;
            _auditLog = auditLog;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<VendorDto>> GetAllAsync()
        {
            return await _context.Vendors
                .Where(x => !x.IsDeleted)
                .Select(VendorMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<VendorDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Vendors
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.VendorName)
                .Select(VendorMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<VendorDto?> GetByIdAsync(Guid id)
        {
            return await _context.Vendors
                .Where(x => x.VendorId == id && !x.IsDeleted)
                .Select(VendorMappings.ToDtoExpr)
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<VendorDto> CreateAsync(VendorDto dto)
        {
            var entity = new Vendor
            {
                VendorId = Guid.NewGuid(),
                VendorCode = dto.VendorCode,
                VendorName = dto.VendorName,
                ShortName = dto.ShortName,
                VendorCategoryId = dto.VendorCategoryId,
                OwnerName = dto.OwnerName,
                OwnerPhone = dto.OwnerPhone,
                CompanyEmail = dto.CompanyEmail,
                NPWP = dto.NPWP,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
                IsDeleted = false
            };

            _context.Vendors.Add(entity);
            await _context.SaveChangesAsync();

            var result = entity.ToDto();

            await _auditLog.LogAsync("Vendor", "Create", null, result, entity.VendorId.ToString());
            return result;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(VendorDto dto)
        {
            var entity = await _context.Vendors
                .FirstOrDefaultAsync(x => x.VendorId == dto.VendorId);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.VendorCode = dto.VendorCode;
            entity.VendorName = dto.VendorName;
            entity.ShortName = dto.ShortName;
            entity.VendorCategoryId = dto.VendorCategoryId;
            entity.OwnerName = dto.OwnerName;
            entity.OwnerPhone = dto.OwnerPhone;
            entity.CompanyEmail = dto.CompanyEmail;
            entity.NPWP = dto.NPWP;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = dto.UpdatedBy;
            entity.IsDeleted = dto.IsDeleted;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync("Vendor", "Update", old, entity.ToDto(), entity.VendorId.ToString());
            return true;
        }

        // ========================= DELETE (SOFT) =========================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Vendors
                .FirstOrDefaultAsync(x => x.VendorId == id);

            if (entity == null)
                return false;

            var old = entity.ToDto();

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync("Vendor", "Delete", old, null, id.ToString());
            return true;
        }
    }
}
