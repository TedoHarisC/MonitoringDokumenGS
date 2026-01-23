using MonitoringDokumenGS.Models;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task AssignRoleAsync(Guid userId, int roleId);
    Task RemoveRoleAsync(Guid userId, int roleId);
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
}
