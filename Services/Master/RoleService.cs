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
        return await _context.Roles.ToListAsync();
    }

    public async Task AssignRoleAsync(Guid userId, int roleId)
    {
        var exists = await _context.UserRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId);

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
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);

        if (entity == null)
            throw new KeyNotFoundException("Role not found for user");

        _context.UserRoles.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(x => x.UserId == userId)
            .Join(_context.Roles,
                  ur => ur.RoleId,
                  r => r.RoleId,
                  (ur, r) => r)
            .ToListAsync();
    }
}
