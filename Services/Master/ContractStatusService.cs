using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringDokumenGS.Dtos.Common;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Extensions;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Master
{
    public class ContractStatusService : IContractStatus
    {
        private readonly List<ContractStatusDto> _data = new();
        private readonly object _lock = new();
        private readonly IAuditLog _auditLog;

        public ContractStatusService(IAuditLog auditLog)
        {
            _auditLog = auditLog;
        }

        public Task<IEnumerable<ContractStatusDto>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<ContractStatusDto>>(_data.ToList());
            }
        }

        public Task<PagedResponse<ContractStatusDto>> GetPagedAsync(int page, int pageSize)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.ToPagedResponse(page, pageSize));
            }
        }

        public Task<ContractStatusDto?> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_data.FirstOrDefault(x => x.ContractStatusId == id));
            }
        }

        public async Task<ContractStatusDto> CreateAsync(ContractStatusDto dto)
        {
            lock (_lock)
            {
                dto.ContractStatusId = _data.Count == 0 ? 1 : _data.Max(x => x.ContractStatusId) + 1;
                _data.Add(dto);
            }
            await _auditLog.LogAsync("ContractStatus", "Create", null, dto, dto.ContractStatusId.ToString());
            return dto;
        }

        public async Task<bool> UpdateAsync(ContractStatusDto dto)
        {
            ContractStatusDto? existing;
            ContractStatusDto? old = null;
            lock (_lock)
            {
                existing = _data.FirstOrDefault(x => x.ContractStatusId == dto.ContractStatusId);
                if (existing == null)
                    return false;
                old = new ContractStatusDto
                {
                    ContractStatusId = existing.ContractStatusId,
                    Code = existing.Code,
                    Name = existing.Name
                };
                existing.Code = dto.Code;
                existing.Name = dto.Name;
            }

            if (existing != null && old != null)
                await _auditLog.LogAsync("ContractStatus", "Update", old, existing, existing.ContractStatusId.ToString());

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            ContractStatusDto? removed;
            lock (_lock)
            {
                removed = _data.FirstOrDefault(x => x.ContractStatusId == id);
                if (removed == null)
                    return false;
                _data.Remove(removed);
            }

            await _auditLog.LogAsync("ContractStatus", "Delete", removed, null, id.ToString());
            return true;
        }
    }
}
