using MonitoringDokumenGS.Dtos.Infrastructure;
using MonitoringDokumenGS.Interfaces;
using System;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Services.Infrastructure
{
    /// <summary>
    /// Helper class untuk membuat notifikasi dengan template yang sudah predefined
    /// </summary>
    public class NotificationHelper
    {
        private readonly INotifications _notificationService;

        public NotificationHelper(INotifications notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Kirim notifikasi ke user dengan title dan message custom
        /// </summary>
        public async Task<bool> SendNotificationAsync(Guid userId, string title, string message)
        {
            if (userId == Guid.Empty) return false;

            try
            {
                await _notificationService.CreateAsync(new NotificationDto
                {
                    UserId = userId,
                    Title = title,
                    Message = message
                });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send notification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kirim notifikasi invoice created
        /// </summary>
        public async Task<bool> NotifyInvoiceCreatedAsync(Guid userId, string invoiceNumber, decimal amount)
        {
            return await SendNotificationAsync(
                userId,
                "Invoice Created",
                $"Invoice {invoiceNumber} has been successfully created with amount {amount:N2}"
            );
        }

        /// <summary>
        /// Kirim notifikasi invoice updated
        /// </summary>
        public async Task<bool> NotifyInvoiceUpdatedAsync(Guid userId, string invoiceNumber)
        {
            return await SendNotificationAsync(
                userId,
                "Invoice Updated",
                $"Invoice {invoiceNumber} has been updated"
            );
        }

        /// <summary>
        /// Kirim notifikasi invoice approved
        /// </summary>
        public async Task<bool> NotifyInvoiceApprovedAsync(Guid userId, string invoiceNumber)
        {
            return await SendNotificationAsync(
                userId,
                "Invoice Approved",
                $"Invoice {invoiceNumber} has been approved"
            );
        }

        /// <summary>
        /// Kirim notifikasi contract created
        /// </summary>
        public async Task<bool> NotifyContractCreatedAsync(Guid userId, string contractNumber, DateTime startDate, DateTime endDate)
        {
            return await SendNotificationAsync(
                userId,
                "Contract Created",
                $"Contract {contractNumber} has been successfully created from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
            );
        }

        /// <summary>
        /// Kirim notifikasi contract updated
        /// </summary>
        public async Task<bool> NotifyContractUpdatedAsync(Guid userId, string contractNumber)
        {
            return await SendNotificationAsync(
                userId,
                "Contract Updated",
                $"Contract {contractNumber} has been updated"
            );
        }

        /// <summary>
        /// Kirim notifikasi contract approved
        /// </summary>
        public async Task<bool> NotifyContractApprovedAsync(Guid userId, string contractNumber)
        {
            return await SendNotificationAsync(
                userId,
                "Contract Approved",
                $"Contract {contractNumber} has been approved"
            );
        }

        /// <summary>
        /// Kirim notifikasi attachment uploaded
        /// </summary>
        public async Task<bool> NotifyAttachmentUploadedAsync(Guid userId, string fileName, string module)
        {
            return await SendNotificationAsync(
                userId,
                "File Uploaded",
                $"File {fileName} has been uploaded to {module}"
            );
        }

        /// <summary>
        /// Kirim notifikasi ke multiple users
        /// </summary>
        public async Task<int> SendNotificationToMultipleUsersAsync(Guid[] userIds, string title, string message)
        {
            int successCount = 0;

            foreach (var userId in userIds)
            {
                bool success = await SendNotificationAsync(userId, title, message);
                if (success) successCount++;
            }

            return successCount;
        }
    }
}
