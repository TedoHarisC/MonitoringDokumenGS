using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Models;

[Authorize(Roles = "SUPER_ADMIN")]
[ApiController]
[Route("api/roles")]
public class RolesApiController : ControllerBase
{
    private readonly IRoleService _service;
    private readonly ApplicationDBContext _context;

    public RolesApiController(IRoleService service, ApplicationDBContext context)
    {
        _service = service;
        _context = context;
    }

    [HttpGet("{roleId:int}")]
    public async Task<IActionResult> GetById([FromRoute] int roleId)
    {
        var role = await _service.GetByIdAsync(roleId);
        if (role == null) return NotFound();
        return Ok(role);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllRolesAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Role role)
    {
        var created = await _service.CreateAsync(role);
        return CreatedAtAction(nameof(GetById), new { roleId = created.RoleId }, created);
    }

    [HttpPut("{roleId:int}")]
    public async Task<IActionResult> Update([FromRoute] int roleId, [FromBody] Role role)
    {
        if (role == null) return BadRequest();
        role.RoleId = roleId;
        var ok = await _service.UpdateAsync(role);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{roleId:int}")]
    public async Task<IActionResult> Delete([FromRoute] int roleId)
    {
        var ok = await _service.DeleteAsync(roleId);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(Guid userId, int roleId)
    {
        await _service.AssignRoleAsync(userId, roleId);
        return Ok();
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove(Guid userId, int roleId)
    {
        await _service.RemoveRoleAsync(userId, roleId);
        return Ok();
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        return Ok(await _service.GetUserRolesAsync(userId));
    }

    // Only users with ADMIN/SUPER_ADMIN role
    [HttpGet("admin-users")]
    public async Task<IActionResult> GetAdminUsers()
    {
        var adminCodes = new[] { "ADMIN", "SUPER_ADMIN" };

        var adminUserIds = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => !ur.IsDeleted)
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.RoleId,
                (ur, r) => new { ur.UserId, r.Code })
            .Where(x => adminCodes.Contains(x.Code))
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync();

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => adminUserIds.Contains(u.UserId) && !u.isDeleted)
            .OrderBy(u => u.Username)
            .ToListAsync();

        var rolesByUser = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => adminUserIds.Contains(ur.UserId) && !ur.IsDeleted)
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.RoleId,
                (ur, r) => new { ur.UserId, Role = r })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.Role).ToList());

        var result = users.Select(u => new
        {
            userId = u.UserId,
            vendorId = u.VendorId,
            username = u.Username,
            email = u.Email,
            isActive = u.isActive,
            roles = rolesByUser.TryGetValue(u.UserId, out var rs) ? rs : new List<Role>()
        });

        return Ok(result);
    }
}
