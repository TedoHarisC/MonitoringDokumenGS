using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Infrastructure;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Mappings.Infrastructure;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Services.Infrastructure
{
    public class NotificationService : INotifications
    {
        private readonly ApplicationDBContext _context;

        public NotificationService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationDto>> GetForUserAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(NotificationMappings.ToDtoExpr)
                .ToListAsync();
        }

        public async Task<NotificationDto?> GetByIdAsync(Guid id)
        {
            var notification = await _context.Notifications
                .Where(n => n.NotificationId == id)
                .Select(NotificationMappings.ToDtoExpr)
                .FirstOrDefaultAsync();

            return notification;
        }

        public async Task<NotificationDto> CreateAsync(NotificationDto dto)
        {
            var notification = new Notifications
            {
                NotificationId = Guid.NewGuid(),
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification.ToDto();
        }

        public async Task<bool> MarkAsReadAsync(Guid id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
