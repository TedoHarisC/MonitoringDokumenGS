using MonitoringDokumenGS.Dtos.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Interfaces
{
    public interface INotifications
    {
        Task<IEnumerable<NotificationDto>> GetForUserAsync(Guid userId);
        Task<NotificationDto?> GetByIdAsync(Guid id);
        Task<NotificationDto> CreateAsync(NotificationDto dto);
        Task<bool> MarkAsReadAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
    }
}
