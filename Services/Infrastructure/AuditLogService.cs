using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Infrastructure;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Services.Infrastructure
{
    public class AuditLogService : IAuditLog
    {
        private readonly ApplicationDBContext _context;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IDbConnection _dbConnection;

        public AuditLogService(ApplicationDBContext context, IHttpContextAccessor httpContextAccessor, IDbConnection dbConnection)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _dbConnection = dbConnection;
        }

        public Task<IEnumerable<AuditLogDto>> GetAllAsync()
        {
            return GetAllInternalAsync();
        }

        private async Task<IEnumerable<AuditLogDto>> GetAllInternalAsync()
        {
            var items = await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Take(1000)
                .ToListAsync();

            return items.Select(ToDto).ToList();
        }

        public async Task<AuditLogDto?> GetByIdAsync(Guid id)
        {
            var entity = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AuditLogId == id);

            return entity == null ? null : ToDto(entity);
        }

        public async Task<AuditLogDto> CreateAsync(AuditLogDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = new AuditLog
            {
                AuditLogId = dto.AuditLogId == Guid.Empty ? Guid.NewGuid() : dto.AuditLogId,
                UserId = dto.UserId,
                EntityName = dto.EntityName ?? string.Empty,
                EntityId = dto.EntityId,
                OldData = dto.OldData ?? string.Empty,
                NewData = dto.NewData ?? string.Empty,
                CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt
            };

            _context.AuditLogs.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        public Task LogAsync(string entityName, string action, object? oldData, object? newData, string entityKey)
        {
            var combinedName = string.IsNullOrWhiteSpace(action)
                ? entityName
                : $"{entityName}:{action}";

            Guid userId = Guid.Empty;

            var userClaim = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim != null && Guid.TryParse(userClaim.Value, out var uid))
                userId = uid;

            var dto = new AuditLogDto
            {
                AuditLogId = Guid.NewGuid(),
                UserId = userId,
                EntityName = combinedName,
                EntityId = entityKey, // ← langsung simpan
                OldData = oldData is null ? string.Empty : JsonSerializer.Serialize(oldData),
                NewData = newData is null ? string.Empty : JsonSerializer.Serialize(newData),
                CreatedAt = DateTime.UtcNow
            };

            return CreateAsync(dto);
        }


        private static AuditLogDto ToDto(AuditLog entity)
        {
            return new AuditLogDto
            {
                AuditLogId = entity.AuditLogId,
                UserId = entity.UserId,
                EntityName = entity.EntityName ?? string.Empty,
                EntityId = entity.EntityId,
                OldData = entity.OldData ?? string.Empty,
                NewData = entity.NewData ?? string.Empty,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<List<AuditHistoryDto>> GetAuditHistoryAsync(string entityId, string entityType)
        {
            var sql = @"
                SELECT 
                    EntityType,
                    EntityId,
                    DocumentNumber,
                    StatusTransition,
                    Username,
                    CreatedAt,
                    Jenis
                FROM vw_AuditStatusChanges
                WHERE EntityId = @EntityId AND EntityType = @EntityType
                ORDER BY CreatedAt DESC";

            var result = await _dbConnection.QueryAsync<AuditHistoryDto>(sql, new { EntityId = entityId, EntityType = entityType });
            return result.ToList();
        }
    }
}
