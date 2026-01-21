using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class Notifications
    {
        [Key]
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

        // Navigation properties
        public UserModel User { get; set; } = default!;
    }
}