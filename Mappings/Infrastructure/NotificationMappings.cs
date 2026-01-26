using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Infrastructure;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Infrastructure
{
    public static class NotificationMappings
    {
        public static Expression<Func<Notifications, NotificationDto>> ToDtoExpr =>
            n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            };

        public static NotificationDto ToDto(this Notifications notification)
        {
            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            };
        }
    }
}
