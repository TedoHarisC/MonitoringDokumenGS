using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;
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
                .Select(ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<VendorDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Vendors
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.VendorName)
                .Select(ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<VendorDto?> GetByIdAsync(Guid id)
        {
            return await _context.Vendors
                .Where(x => x.VendorId == id && !x.IsDeleted)
                .Select(ToDtoExpr)
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

            var result = ToDto(entity);

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

            var old = ToDto(entity);

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

            await _auditLog.LogAsync("Vendor", "Update", old, ToDto(entity), entity.VendorId.ToString());
            return true;
        }

        // ========================= DELETE (SOFT) =========================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Vendors
                .FirstOrDefaultAsync(x => x.VendorId == id);

            if (entity == null)
                return false;

            var old = ToDto(entity);

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync("Vendor", "Delete", old, null, id.ToString());
            return true;
        }

        // ========================= MAPPER EF =========================
        private static readonly Expression<Func<Vendor, VendorDto>> ToDtoExpr =
            x => new VendorDto
            {
                VendorId = x.VendorId,
                VendorCode = x.VendorCode,
                VendorName = x.VendorName,
                ShortName = x.ShortName,
                VendorCategoryId = x.VendorCategoryId,
                OwnerName = x.OwnerName,
                OwnerPhone = x.OwnerPhone,
                CompanyEmail = x.CompanyEmail,
                NPWP = x.NPWP,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy,
                IsDeleted = x.IsDeleted
            };

        // ========================= MAPPER MEMORY =========================
        private static VendorDto ToDto(Vendor x)
        {
            return new VendorDto
            {
                VendorId = x.VendorId,
                VendorCode = x.VendorCode,
                VendorName = x.VendorName,
                ShortName = x.ShortName,
                VendorCategoryId = x.VendorCategoryId,
                OwnerName = x.OwnerName,
                OwnerPhone = x.OwnerPhone,
                CompanyEmail = x.CompanyEmail,
                NPWP = x.NPWP,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy,
                IsDeleted = x.IsDeleted
            };
        }
    }
}
