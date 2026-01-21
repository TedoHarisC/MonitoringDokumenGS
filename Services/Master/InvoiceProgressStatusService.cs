using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Master
{
    public class InvoiceProgressStatusService : IInvoiceProgressStatuses
    {
        private readonly List<InvoiceProgressStatusDto> _data = new();
        private readonly object _lock = new();
        private readonly IAuditLog _auditLog;

        public InvoiceProgressStatusService(IAuditLog auditLog)
        {
            _auditLog = auditLog;
        }

        public Task<IEnumerable<InvoiceProgressStatusDto>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<InvoiceProgressStatusDto>>(_data.ToList());
            }
        }

        public Task<PagedResponse<InvoiceProgressStatusDto>> GetPagedAsync(int page, int pageSize)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.ToPagedResponse(page, pageSize));
            }
        }

        public Task<InvoiceProgressStatusDto?> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.FirstOrDefault(x => x.ProgressStatusId == id));
            }
        }

        public async Task<InvoiceProgressStatusDto> CreateAsync(InvoiceProgressStatusDto dto)
        {
            lock (_lock)
            {
                dto.ProgressStatusId = _data.Count == 0 ? 1 : _data.Max(x => x.ProgressStatusId) + 1;
                _data.Add(dto);
            }
            await _auditLog.LogAsync("InvoiceProgressStatus", "Create", null, dto, dto.ProgressStatusId.ToString());
            return dto;
        }

        public async Task<bool> UpdateAsync(InvoiceProgressStatusDto dto)
        {
            InvoiceProgressStatusDto? existing;
            InvoiceProgressStatusDto? old = null;
            lock (_lock)
            {
                existing = _data.FirstOrDefault(x => x.ProgressStatusId == dto.ProgressStatusId);
                if (existing == null)
                    return false;
                old = new InvoiceProgressStatusDto
                {
                    ProgressStatusId = existing.ProgressStatusId,
                    Code = existing.Code,
                    Name = existing.Name
                };
                existing.Code = dto.Code;
                existing.Name = dto.Name;
            }

            if (existing != null && old != null)
                await _auditLog.LogAsync("InvoiceProgressStatus", "Update", old, existing, existing.ProgressStatusId.ToString());

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            InvoiceProgressStatusDto? removed;
            lock (_lock)
            {
                removed = _data.FirstOrDefault(x => x.ProgressStatusId == id);
                if (removed == null)
                    return false;
                _data.Remove(removed);
            }

            await _auditLog.LogAsync("InvoiceProgressStatus", "Delete", removed, null, id.ToString());
            return true;
        }
    }
}
