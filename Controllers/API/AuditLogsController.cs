using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize(Roles = "SUPER_ADMIN, ADMIN")]
    [ApiController]
    [Route("api/audit-logs")]
    public class AuditLogsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public AuditLogsController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? q = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 50;
            if (pageSize > 500) pageSize = 500;

            var query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    x.EntityName.Contains(term) ||
                    x.OldData.Contains(term) ||
                    x.NewData.Contains(term));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    auditLogId = x.AuditLogId,
                    createdAt = x.CreatedAt,
                    userId = x.UserId,
                    entityName = x.EntityName,
                    entityId = x.EntityId,
                    oldData = x.OldData,
                    newData = x.NewData
                })
                .ToListAsync();

            // Join username in-memory to avoid FK assumptions.
            var userIds = items.Select(x => x.userId).Where(x => x != Guid.Empty).Distinct().ToList();
            var usernames = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .Select(u => new { u.UserId, u.Username })
                .ToDictionaryAsync(x => x.UserId, x => x.Username);

            var result = items.Select(x => new
            {
                x.auditLogId,
                x.createdAt,
                x.userId,
                username = (x.userId != Guid.Empty && usernames.TryGetValue(x.userId, out var un)) ? un : string.Empty,
                x.entityName,
                x.entityId,
                x.oldData,
                x.newData
            });

            return Ok(new { items = result, total, page, pageSize });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var item = await _context.AuditLogs
                .AsNoTracking()
                .Where(x => x.AuditLogId == id)
                .Select(x => new
                {
                    auditLogId = x.AuditLogId,
                    createdAt = x.CreatedAt,
                    userId = x.UserId,
                    entityName = x.EntityName,
                    entityId = x.EntityId,
                    oldData = x.OldData,
                    newData = x.NewData
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound();

            var username = string.Empty;
            if (item.userId != Guid.Empty)
                username = await _context.Users.AsNoTracking().Where(u => u.UserId == item.userId).Select(u => u.Username).FirstOrDefaultAsync() ?? string.Empty;

            return Ok(new
            {
                item.auditLogId,
                item.createdAt,
                item.userId,
                username,
                item.entityName,
                item.entityId,
                item.oldData,
                item.newData
            });
        }
    }
}
