using MonitoringDokumenGS.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IUserRefreshToken
    {
        Task<IEnumerable<UserRefreshTokenDto>> GetAllAsync();
        Task<UserRefreshTokenDto?> GetByIdAsync(Guid id);
        Task<UserRefreshTokenDto> CreateAsync(UserRefreshTokenDto dto);
        Task<bool> UpdateAsync(UserRefreshTokenDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
