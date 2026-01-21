using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Master
{
    public class VendorService : IVendor
    {
        private readonly List<VendorDto> _data = new();
        private readonly object _lock = new();
        private readonly IAuditLog _auditLog;

        public VendorService(IAuditLog auditLog)
        {
            _auditLog = auditLog;
        }

        public Task<IEnumerable<VendorDto>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<VendorDto>>(_data.ToList());
            }
        }

        public Task<PagedResponse<VendorDto>> GetPagedAsync(int page, int pageSize)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.ToPagedResponse(page, pageSize));
            }
        }

        public Task<VendorDto?> GetByIdAsync(Guid id)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.FirstOrDefault(x => x.VendorId == id));
            }
        }

        public async Task<VendorDto> CreateAsync(VendorDto dto)
        {
            lock (_lock)
            {
                dto.VendorId = dto.VendorId == Guid.Empty ? Guid.NewGuid() : dto.VendorId;
                dto.CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt;
                dto.UpdatedAt = dto.UpdatedAt == default ? dto.CreatedAt : dto.UpdatedAt;
                _data.Add(dto);
            }
            await _auditLog.LogAsync("Vendor", "Create", null, dto, dto.VendorId.ToString());
            return dto;
        }

        public async Task<bool> UpdateAsync(VendorDto dto)
        {
            VendorDto? existing;
            VendorDto? old = null;
            lock (_lock)
            {
                existing = _data.FirstOrDefault(x => x.VendorId == dto.VendorId);
                if (existing == null)
                    return false;
                old = Clone(existing);
                existing.VendorCode = dto.VendorCode;
                existing.VendorName = dto.VendorName;
                existing.ShortName = dto.ShortName;
                existing.VendorCategoryId = dto.VendorCategoryId;
                existing.OwnerName = dto.OwnerName;
                existing.OwnerPhone = dto.OwnerPhone;
                existing.CompanyEmail = dto.CompanyEmail;
                existing.NPWP = dto.NPWP;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = dto.UpdatedBy;
                existing.IsDeleted = dto.IsDeleted;
            }

            if (existing != null && old != null)
                await _auditLog.LogAsync("Vendor", "Update", old, existing, existing.VendorId.ToString());

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            VendorDto? removed;
            lock (_lock)
            {
                removed = _data.FirstOrDefault(x => x.VendorId == id);
                if (removed == null)
                    return false;
                _data.Remove(removed);
            }

            await _auditLog.LogAsync("Vendor", "Delete", removed, null, id.ToString());
            return true;
        }

        private static VendorDto Clone(VendorDto source)
        {
            return new VendorDto
            {
                VendorId = source.VendorId,
                VendorCode = source.VendorCode,
                VendorName = source.VendorName,
                ShortName = source.ShortName,
                VendorCategoryId = source.VendorCategoryId,
                OwnerName = source.OwnerName,
                OwnerPhone = source.OwnerPhone,
                CompanyEmail = source.CompanyEmail,
                NPWP = source.NPWP,
                CreatedAt = source.CreatedAt,
                CreatedBy = source.CreatedBy,
                UpdatedAt = source.UpdatedAt,
                UpdatedBy = source.UpdatedBy,
                IsDeleted = source.IsDeleted
            };
        }
    }
}
