using MonitoringDokumenGS.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IUser
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<UserDto?> GetByUsernameAsync(string username);
        Task<UserDto> CreateAsync(UserDto dto);
        Task<bool> UpdateAsync(UserDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
