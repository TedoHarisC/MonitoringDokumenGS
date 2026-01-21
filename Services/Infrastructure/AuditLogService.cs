using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MonitoringDokumenGS.Dtos.Infrastructure;
using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Infrastructure
{
    public class AuditLogService : IAuditLog
    {
        private readonly List<AuditLogDto> _logs = new();
        private readonly object _lock = new();

        public Task<IEnumerable<AuditLogDto>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<AuditLogDto>>(_logs.ToList());
            }
        }

        public Task<AuditLogDto?> GetByIdAsync(Guid id)
        {
            lock (_lock)
            {
                var log = _logs.FirstOrDefault(l => l.AuditLogId == id);
                return Task.FromResult(log);
            }
        }

        public Task<AuditLogDto> CreateAsync(AuditLogDto dto)
        {
            lock (_lock)
            {
                dto.AuditLogId = dto.AuditLogId == Guid.Empty ? Guid.NewGuid() : dto.AuditLogId;
                dto.CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt;
                _logs.Add(dto);
                return Task.FromResult(dto);
            }
        }

        public Task LogAsync(string entityName, string action, object? oldData, object? newData, string entityKey)
        {
            var dto = new AuditLogDto
            {
                AuditLogId = Guid.NewGuid(),
                UserId = Guid.Empty,
                EntityName = entityName,
                EntityId = Guid.NewGuid(),
                OldData = oldData is null ? string.Empty : JsonSerializer.Serialize(oldData),
                NewData = newData is null ? string.Empty : JsonSerializer.Serialize(newData),
                CreatedAt = DateTime.UtcNow
            };
            return CreateAsync(dto);
        }
    }
}
