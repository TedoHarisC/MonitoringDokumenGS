using MonitoringDokumenGS.Models;

public interface IRoleService
{
    Task<Role?> GetByIdAsync(int roleId);
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role> CreateAsync(Role role);
    Task<bool> UpdateAsync(Role role);
    Task<bool> DeleteAsync(int roleId);
    Task AssignRoleAsync(Guid userId, int roleId);
    Task RemoveRoleAsync(Guid userId, int roleId);
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
}
