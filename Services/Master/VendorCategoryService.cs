using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Master
{
    public class VendorCategoryService : IVendorCategory
    {
        private readonly List<VendorCategoryDto> _data = new();
        private readonly object _lock = new();
        private readonly IAuditLog _auditLog;

        public VendorCategoryService(IAuditLog auditLog)
        {
            _auditLog = auditLog;
        }

        public Task<IEnumerable<VendorCategoryDto>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<VendorCategoryDto>>(_data.ToList());
            }
        }

        public Task<PagedResponse<VendorCategoryDto>> GetPagedAsync(int page, int pageSize)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.ToPagedResponse(page, pageSize));
            }
        }

        public Task<VendorCategoryDto?> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.FirstOrDefault(x => x.VendorCategoryId == id));
            }
        }

        public async Task<VendorCategoryDto> CreateAsync(VendorCategoryDto dto)
        {
            lock (_lock)
            {
                dto.VendorCategoryId = _data.Count == 0 ? 1 : _data.Max(x => x.VendorCategoryId) + 1;
                dto.CreatedAt = dto.CreatedAt == default ? System.DateTime.UtcNow : dto.CreatedAt;
                _data.Add(dto);
            }
            await _auditLog.LogAsync("VendorCategory", "Create", null, dto, dto.VendorCategoryId.ToString());
            return dto;
        }

        public async Task<bool> UpdateAsync(VendorCategoryDto dto)
        {
            VendorCategoryDto? existing;
            VendorCategoryDto? old = null;
            lock (_lock)
            {
                existing = _data.FirstOrDefault(x => x.VendorCategoryId == dto.VendorCategoryId);
                if (existing == null)
                    return false;
                old = new VendorCategoryDto
                {
                    VendorCategoryId = existing.VendorCategoryId,
                    Name = existing.Name,
                    CreatedAt = existing.CreatedAt
                };
                existing.Name = dto.Name;
            }

            if (existing != null && old != null)
                await _auditLog.LogAsync("VendorCategory", "Update", old, existing, existing.VendorCategoryId.ToString());

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            VendorCategoryDto? removed;
            lock (_lock)
            {
                removed = _data.FirstOrDefault(x => x.VendorCategoryId == id);
                if (removed == null)
                    return false;
                _data.Remove(removed);
            }

            await _auditLog.LogAsync("VendorCategory", "Delete", removed, null, id.ToString());
            return true;
        }
    }
}
