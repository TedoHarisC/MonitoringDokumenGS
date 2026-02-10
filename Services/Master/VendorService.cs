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
                .Include(v => v.VendorPics)
                .Where(x => !x.IsDeleted)
                .Select(VendorMappings.ToDtoExpr)
                .ToListAsync();
        }

        // ========================= PAGING =========================
        public async Task<PagedResponse<VendorDto>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Vendors
                .Include(v => v.VendorPics)
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.VendorName)
                .Select(VendorMappings.ToDtoExpr)
                .ToPagedResponseAsync(page, pageSize);
        }

        // ========================= GET BY ID =========================
        public async Task<VendorDto?> GetByIdAsync(Guid id)
        {
            return await _context.Vendors
                .Include(v => v.VendorPics)
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

            // Add VendorPics if provided
            if (dto.VendorPics != null && dto.VendorPics.Any())
            {
                foreach (var picDto in dto.VendorPics)
                {
                    var pic = new VendorPics
                    {
                        VendorPicId = Guid.NewGuid(),
                        VendorId = entity.VendorId,
                        PicName = picDto.PicName,
                        PicNumber = picDto.PicNumber,
                        PicEmail = picDto.PicEmail,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = dto.CreatedBy,
                        IsDeleted = false
                    };
                    entity.VendorPics.Add(pic);
                }
            }

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

            // Load old data for audit (with tracking disabled for this query)
            var oldDto = await _context.Vendors
                .Include(v => v.VendorPics)
                .AsNoTracking()
                .Where(x => x.VendorId == dto.VendorId)
                .Select(VendorMappings.ToDtoExpr)
                .FirstOrDefaultAsync();

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

            // Handle VendorPics updates - load separately to avoid tracking issues
            var existingPics = await _context.Set<VendorPics>()
                .Where(p => p.VendorId == entity.VendorId)
                .ToListAsync();

            if (dto.VendorPics != null)
            {
                var incomingPicIds = dto.VendorPics
                    .Where(p => p.VendorPicId != Guid.Empty)
                    .Select(p => p.VendorPicId)
                    .ToList();

                // Soft delete removed PICs
                foreach (var existingPic in existingPics.Where(p => !p.IsDeleted))
                {
                    if (!incomingPicIds.Contains(existingPic.VendorPicId))
                    {
                        existingPic.IsDeleted = true;
                        existingPic.UpdatedAt = DateTime.UtcNow;
                        existingPic.UpdatedBy = dto.UpdatedBy;
                    }
                }

                // Update or Add PICs
                foreach (var picDto in dto.VendorPics)
                {
                    if (picDto.VendorPicId == Guid.Empty)
                    {
                        // New PIC
                        var newPic = new VendorPics
                        {
                            VendorPicId = Guid.NewGuid(),
                            VendorId = entity.VendorId,
                            PicName = picDto.PicName,
                            PicNumber = picDto.PicNumber,
                            PicEmail = picDto.PicEmail,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = dto.UpdatedBy,
                            IsDeleted = false
                        };
                        _context.Set<VendorPics>().Add(newPic);
                    }
                    else
                    {
                        // Update existing PIC
                        var existingPic = existingPics.FirstOrDefault(p => p.VendorPicId == picDto.VendorPicId);
                        if (existingPic != null)
                        {
                            existingPic.PicName = picDto.PicName;
                            existingPic.PicNumber = picDto.PicNumber;
                            existingPic.PicEmail = picDto.PicEmail;
                            existingPic.UpdatedAt = DateTime.UtcNow;
                            existingPic.UpdatedBy = dto.UpdatedBy;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            await _auditLog.LogAsync("Vendor", "Update", oldDto, entity.ToDto(), entity.VendorId.ToString());
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
