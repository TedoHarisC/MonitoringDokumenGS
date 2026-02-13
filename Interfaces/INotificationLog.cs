using MonitoringDokumenGS.Models.Infrastructure;

namespace MonitoringDokumenGS.Interfaces
{
    public interface INotificationLog
    {
        /// <summary>
        /// Check if notification should be sent (anti-spam check)
        /// </summary>
        Task<bool> CanSendNotificationAsync(string notificationType, string referenceId, int cooldownHours = 24);

        /// <summary>
        /// Log notification sent
        /// </summary>
        Task LogNotificationSentAsync(string notificationType, string referenceId, string recipientEmail, int reminderLevel = 0, string details = "");

        /// <summary>
        /// Get last notification log
        /// </summary>
        Task<NotificationLog?> GetLastNotificationAsync(string notificationType, string referenceId);

        /// <summary>
        /// Get reminder level for contract
        /// </summary>
        Task<int> GetContractReminderLevelAsync(string contractId);
    }
}
