using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitoringDokumenGS.Models.Infrastructure
{
    [Table("SYS_NotificationLog")]
    public class NotificationLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ReferenceId { get; set; } = string.Empty;

        [MaxLength(255)]
        public string RecipientEmail { get; set; } = string.Empty;

        public int ReminderLevel { get; set; } = 0;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string Details { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
    }

    // Notification Types Constants
    public static class NotificationTypes
    {
        // Budget
        public const string BUDGET_OVERBUDGET_MONTHLY = "BUDGET_OVERBUDGET_MONTHLY";
        public const string BUDGET_OVERBUDGET_YEARLY = "BUDGET_OVERBUDGET_YEARLY";
        public const string BUDGET_WARNING_MONTHLY = "BUDGET_WARNING_MONTHLY";

        // Contract
        public const string CONTRACT_EXPIRING_60DAYS = "CONTRACT_EXPIRING_60DAYS";
        public const string CONTRACT_EXPIRING_30DAYS = "CONTRACT_EXPIRING_30DAYS";
        public const string CONTRACT_EXPIRING_14DAYS = "CONTRACT_EXPIRING_14DAYS";
        public const string CONTRACT_EXPIRING_7DAYS = "CONTRACT_EXPIRING_7DAYS";
        public const string CONTRACT_EXPIRING_FINAL = "CONTRACT_EXPIRING_FINAL"; // 3,2,1 days
    }
}
