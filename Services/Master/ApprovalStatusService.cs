using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Master
{
    public class ApprovalStatusService : IApprovalStatus
    {
        private readonly List<ApprovalStatusDto> _data = new();
        private readonly object _lock = new();
        private readonly IAuditLog _auditLog;

        public ApprovalStatusService(IAuditLog auditLog)
        {
            _auditLog = auditLog;
        }

        public Task<IEnumerable<ApprovalStatusDto>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<ApprovalStatusDto>>(_data.ToList());
            }
        }

        public Task<PagedResponse<ApprovalStatusDto>> GetPagedAsync(int page, int pageSize)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.ToPagedResponse(page, pageSize));
            }
        }

        public Task<ApprovalStatusDto?> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.FirstOrDefault(x => x.ApprovalStatusId == id));
            }
        }

        public async Task<ApprovalStatusDto> CreateAsync(ApprovalStatusDto dto)
        {
            lock (_lock)
            {
                dto.ApprovalStatusId = _data.Count == 0 ? 1 : _data.Max(x => x.ApprovalStatusId) + 1;
                _data.Add(dto);
            }
            await _auditLog.LogAsync("ApprovalStatus", "Create", null, dto, dto.ApprovalStatusId.ToString());
            return dto;
        }

        public async Task<bool> UpdateAsync(ApprovalStatusDto dto)
        {
            ApprovalStatusDto? existing;
            ApprovalStatusDto? old = null;
            lock (_lock)
            {
                existing = _data.FirstOrDefault(x => x.ApprovalStatusId == dto.ApprovalStatusId);
                if (existing == null)
                    return false;
                old = new ApprovalStatusDto
                {
                    ApprovalStatusId = existing.ApprovalStatusId,
                    Code = existing.Code,
                    Name = existing.Name
                };
                existing.Code = dto.Code;
                existing.Name = dto.Name;
            }

            if (existing != null && old != null)
                await _auditLog.LogAsync("ApprovalStatus", "Update", old, existing, existing.ApprovalStatusId.ToString());

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            ApprovalStatusDto? removed;
            lock (_lock)
            {
                removed = _data.FirstOrDefault(x => x.ApprovalStatusId == id);
                if (removed == null)
                    return false;
                _data.Remove(removed);
            }

            await _auditLog.LogAsync("ApprovalStatus", "Delete", removed, null, id.ToString());
            return true;
        }
    }
}
