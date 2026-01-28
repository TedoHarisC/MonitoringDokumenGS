using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Auth;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models;
using BCryptNet = BCrypt.Net.BCrypt;

[Authorize(Roles = "SUPER_ADMIN, ADMIN")]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUser _service;
    private readonly ApplicationDBContext _context;

    public UsersController(IUser service, ApplicationDBContext context)
    {
        _service = service;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    // Used by Master Users page
    [HttpGet("with-roles")]
    public async Task<IActionResult> GetAllWithRoles()
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => !u.isDeleted)
            .OrderBy(u => u.Username)
            .Select(u => new
            {
                userId = u.UserId,
                vendorId = u.VendorId,
                username = u.Username,
                email = u.Email,
                isActive = u.isActive
            })
            .ToListAsync();

        var vendorNames = await _context.Vendors
            .AsNoTracking()
            .Where(v => !v.IsDeleted)
            .Select(v => new { v.VendorId, v.VendorName })
            .ToDictionaryAsync(x => x.VendorId, x => x.VendorName);

        var userIds = users.Select(x => (Guid)x.userId).ToList();

        var rolesByUser = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => userIds.Contains(ur.UserId) && !ur.IsDeleted)
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.RoleId,
                (ur, r) => new { ur.UserId, Role = r })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.Role).ToList());

        var result = users.Select(u => new
        {
            u.userId,
            u.vendorId,
            vendorName = vendorNames.TryGetValue((Guid)u.vendorId, out var vn) ? vn : string.Empty,
            u.username,
            u.email,
            u.isActive,
            roles = rolesByUser.TryGetValue((Guid)u.userId, out var rs) ? rs : new List<Role>()
        });

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var user = await _service.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.UserId }, created);
    }

    // Used by Master Users page (create with password + role)
    [HttpPost("admin-create")]
    public async Task<IActionResult> AdminCreate([FromBody] AdminCreateUserRequestDto request)
    {
        if (request == null) return BadRequest();
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest(new { message = "Username is required" });
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Password is required" });
        if (request.RoleId <= 0)
            return BadRequest(new { message = "Role is required" });

        // Ditambahkan isDeleted check untuk menghindari pembuatan user duplikat jika user sebelumnya dihapus secara soft delete
        var exists = await _context.Users.AnyAsync(u => (u.Username == request.Username || u.Email == request.Email) && !u.isDeleted);
        if (exists)
            return Conflict(new { message = "Username or email already exists" });

        var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == request.RoleId);
        if (!roleExists)
            return BadRequest(new { message = "Role not found" });

        var vendorId = request.VendorId ?? Guid.Empty;
        if (vendorId != Guid.Empty)
        {
            var vendorExists = await _context.Vendors.AnyAsync(v => v.VendorId == vendorId && !v.IsDeleted);
            if (!vendorExists)
                return BadRequest(new { message = "Vendor not found" });
        }

        var newUserId = Guid.NewGuid();

        var user = new UserModel
        {
            UserId = newUserId,
            VendorId = vendorId,
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = BCryptNet.HashPassword(request.Password),
            isActive = request.IsActive,
            isDeleted = false,
            CreatedBy = newUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // IMPORTANT: ensure Users row exists before inserting UserRoles.
        // Without explicit relationships configured in EF, insert order can cause FK violation.
        await using var trx = await _context.Database.BeginTransactionAsync();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new UserRoles
        {
            UserId = user.UserId,
            RoleId = request.RoleId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = newUserId
        });

        await _context.SaveChangesAsync();
        await trx.CommitAsync();
        return Ok(new { userId = user.UserId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UserDto dto)
    {
        dto.UserId = id;
        var ok = await _service.UpdateAsync(dto);
        return ok ? NoContent() : NotFound();
    }

    // Used by Master Users page (update + optional password + role)
    [HttpPut("admin-update/{id:guid}")]
    public async Task<IActionResult> AdminUpdate([FromRoute] Guid id, [FromBody] AdminUpdateUserRequestDto request)
    {
        if (request == null) return BadRequest();
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest(new { message = "Username is required" });
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required" });
        if (request.RoleId <= 0)
            return BadRequest(new { message = "Role is required" });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();

        var dup = await _context.Users.AnyAsync(u => u.UserId != id && (u.Username == request.Username || u.Email == request.Email) && !u.isDeleted);
        if (dup)
            return Conflict(new { message = "Username or email already exists" });

        var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == request.RoleId);
        if (!roleExists)
            return BadRequest(new { message = "Role not found" });

        var vendorId = request.VendorId ?? Guid.Empty;
        if (vendorId != Guid.Empty)
        {
            var vendorExists = await _context.Vendors.AnyAsync(v => v.VendorId == vendorId && !v.IsDeleted);
            if (!vendorExists)
                return BadRequest(new { message = "Vendor not found" });
        }

        user.Username = request.Username.Trim();
        user.Email = request.Email.Trim();
        user.VendorId = vendorId;
        user.isActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = BCryptNet.HashPassword(request.Password);

        // Make role assignment single-select: soft-delete existing then add selected
        var existing = await _context.UserRoles
            .Where(ur => ur.UserId == id && !ur.IsDeleted)
            .ToListAsync();

        foreach (var ur in existing)
        {
            if (ur.RoleId == request.RoleId) continue;
            ur.IsDeleted = true;
            ur.UpdatedAt = DateTime.UtcNow;
        }

        var hasSelected = existing.Any(ur => ur.RoleId == request.RoleId && !ur.IsDeleted);
        if (!hasSelected)
        {
            _context.UserRoles.Add(new UserRoles
            {
                UserId = id,
                RoleId = request.RoleId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = id
            });
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
