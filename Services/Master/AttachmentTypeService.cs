using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Master
{
    public class AttachmentTypeService : IAttachmentTypes
    {
        private readonly List<AttachmentTypeDto> _data = new();
        private readonly object _lock = new();
        private readonly IAuditLog _auditLog;

        public AttachmentTypeService(IAuditLog auditLog)
        {
            _auditLog = auditLog;
        }

        public Task<IEnumerable<AttachmentTypeDto>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<AttachmentTypeDto>>(_data.ToList());
            }
        }

        public Task<PagedResponse<AttachmentTypeDto>> GetPagedAsync(int page, int pageSize)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.ToPagedResponse(page, pageSize));
            }
        }

        public Task<AttachmentTypeDto?> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.FirstOrDefault(x => x.AttachmentTypeId == id));
            }
        }

        public async Task<AttachmentTypeDto> CreateAsync(AttachmentTypeDto dto)
        {
            lock (_lock)
            {
                dto.AttachmentTypeId = _data.Count == 0 ? 1 : _data.Max(x => x.AttachmentTypeId) + 1;
                dto.CreatedAt = dto.CreatedAt == default ? System.DateTime.UtcNow : dto.CreatedAt;
                _data.Add(dto);
            }
            await _auditLog.LogAsync("AttachmentType", "Create", null, dto, dto.AttachmentTypeId.ToString());
            return dto;
        }

        public async Task<bool> UpdateAsync(AttachmentTypeDto dto)
        {
            AttachmentTypeDto? existing;
            AttachmentTypeDto? old = null;
            lock (_lock)
            {
                existing = _data.FirstOrDefault(x => x.AttachmentTypeId == dto.AttachmentTypeId);
                if (existing == null)
                    return false;
                old = new AttachmentTypeDto
                {
                    AttachmentTypeId = existing.AttachmentTypeId,
                    Code = existing.Code,
                    Name = existing.Name,
                    IsActive = existing.IsActive,
                    AppliesTo = existing.AppliesTo,
                    CreatedAt = existing.CreatedAt
                };
                existing.Code = dto.Code;
                existing.Name = dto.Name;
                existing.IsActive = dto.IsActive;
                existing.AppliesTo = dto.AppliesTo;
            }

            if (existing != null && old != null)
                await _auditLog.LogAsync("AttachmentType", "Update", old, existing, existing.AttachmentTypeId.ToString());

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            AttachmentTypeDto? removed;
            lock (_lock)
            {
                removed = _data.FirstOrDefault(x => x.AttachmentTypeId == id);
                if (removed == null)
                    return false;
                _data.Remove(removed);
            }

            await _auditLog.LogAsync("AttachmentType", "Delete", removed, null, id.ToString());
            return true;
        }
    }
}
