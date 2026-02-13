using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models.Infrastructure;

namespace MonitoringDokumenGS.Services.Infrastructure
{
    public class NotificationLogService : INotificationLog
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<NotificationLogService> _logger;

        public NotificationLogService(ApplicationDBContext context, ILogger<NotificationLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CanSendNotificationAsync(string notificationType, string referenceId, int cooldownHours = 24)
        {
            try
            {
                var cutoffTime = DateTime.Now.AddHours(-cooldownHours);

                var recentNotification = await _context.SYS_NotificationLog
                    .Where(n => n.NotificationType == notificationType
                                && n.ReferenceId == referenceId
                                && n.SentAt >= cutoffTime
                                && !n.IsDeleted)
                    .OrderByDescending(n => n.SentAt)
                    .FirstOrDefaultAsync();

                // Jika tidak ada notifikasi recent, boleh kirim
                if (recentNotification == null)
                {
                    return true;
                }

                // Jika ada notifikasi dalam cooldown period, jangan kirim
                _logger.LogInformation(
                    "Notification blocked by cooldown. Type: {Type}, Ref: {Ref}, Last sent: {LastSent}",
                    notificationType, referenceId, recentNotification.SentAt);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking notification cooldown");
                // Default allow if error
                return true;
            }
        }

        public async Task LogNotificationSentAsync(string notificationType, string referenceId, string recipientEmail, int reminderLevel = 0, string details = "")
        {
            try
            {
                var log = new NotificationLog
                {
                    NotificationType = notificationType,
                    ReferenceId = referenceId,
                    RecipientEmail = recipientEmail,
                    ReminderLevel = reminderLevel,
                    SentAt = DateTime.Now,
                    Details = details,
                    IsDeleted = false
                };

                _context.SYS_NotificationLog.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Notification logged. Type: {Type}, Ref: {Ref}, Level: {Level}",
                    notificationType, referenceId, reminderLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging notification");
            }
        }

        public async Task<NotificationLog?> GetLastNotificationAsync(string notificationType, string referenceId)
        {
            try
            {
                return await _context.SYS_NotificationLog
                    .Where(n => n.NotificationType == notificationType
                                && n.ReferenceId == referenceId
                                && !n.IsDeleted)
                    .OrderByDescending(n => n.SentAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last notification");
                return null;
            }
        }

        public async Task<int> GetContractReminderLevelAsync(string contractId)
        {
            try
            {
                var lastNotification = await _context.SYS_NotificationLog
                    .Where(n => n.ReferenceId == contractId
                                && n.NotificationType.StartsWith("CONTRACT_EXPIRING")
                                && !n.IsDeleted)
                    .OrderByDescending(n => n.ReminderLevel)
                    .FirstOrDefaultAsync();

                return lastNotification?.ReminderLevel ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract reminder level");
                return 0;
            }
        }
    }
}
