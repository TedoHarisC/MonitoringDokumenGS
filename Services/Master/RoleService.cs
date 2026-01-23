using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Models;

public class RoleService : IRoleService
{
    private readonly ApplicationDBContext _context;

    public RoleService(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles
            .OrderBy(r => r.Code)
            .ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(int roleId)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<Role> CreateAsync(Role role)
    {
        if (role is null) throw new ArgumentNullException(nameof(role));
        if (string.IsNullOrWhiteSpace(role.Code))
            throw new ArgumentException("Role Code is required.", nameof(role));

        var exists = await _context.Roles.AnyAsync(r => r.Code == role.Code);
        if (exists)
            throw new InvalidOperationException("Role code already exists");

        var entity = new Role
        {
            Code = role.Code.Trim(),
            Name = (role.Name ?? string.Empty).Trim(),
        };

        _context.Roles.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        if (role is null) throw new ArgumentNullException(nameof(role));

        var entity = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == role.RoleId);
        if (entity == null) return false;

        if (!string.IsNullOrWhiteSpace(role.Code) && !string.Equals(entity.Code, role.Code, StringComparison.Ordinal))
        {
            var codeExists = await _context.Roles.AnyAsync(r => r.RoleId != role.RoleId && r.Code == role.Code);
            if (codeExists)
                throw new InvalidOperationException("Role code already exists");
            entity.Code = role.Code.Trim();
        }

        entity.Name = (role.Name ?? string.Empty).Trim();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int roleId)
    {
        var entity = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
        if (entity == null) return false;

        // prevent delete when still used
        var inUse = await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId && !ur.IsDeleted);
        if (inUse)
            throw new InvalidOperationException("Role is still assigned to users");

        _context.Roles.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task AssignRoleAsync(Guid userId, int roleId)
    {
        var exists = await _context.UserRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId && !x.IsDeleted);

        if (exists)
            throw new InvalidOperationException("Role already assigned");

        _context.UserRoles.Add(new UserRoles
        {
            UserId = userId,
            RoleId = roleId
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveRoleAsync(Guid userId, int roleId)
    {
        var entity = await _context.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId && !x.IsDeleted);

        if (entity == null)
            throw new KeyNotFoundException("Role not found for user");

        _context.UserRoles.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Join(_context.Roles,
                  ur => ur.RoleId,
                  r => r.RoleId,
                  (ur, r) => r)
            .ToListAsync();
    }
}
