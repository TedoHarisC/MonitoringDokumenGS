using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Dtos.Infrastructure;
using MonitoringDokumenGS.Interfaces;
using System.Security.Claims;

namespace MonitoringDokumenGS.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotifications _notificationService;

        public NotificationsController(INotifications notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get notifications for current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetForCurrentUser()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var notifications = await _notificationService.GetForUserAsync(userId);
                return Ok(new { success = true, data = notifications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving notifications: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get unread count for current user
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var notifications = await _notificationService.GetForUserAsync(userId);
                var unreadCount = notifications.Count(n => !n.IsRead);

                return Ok(new { success = true, data = new { count = unreadCount } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving unread count: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var notification = await _notificationService.GetByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new { success = false, message = "Notification not found" });
                }

                return Ok(new { success = true, data = notification });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving notification: {ex.Message}" });
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                var success = await _notificationService.MarkAsReadAsync(id);
                if (!success)
                {
                    return NotFound(new { success = false, message = "Notification not found" });
                }

                return Ok(new { success = true, message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error marking notification: {ex.Message}" });
            }
        }

        /// <summary>
        /// Mark all notifications as read for current user
        /// </summary>
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var notifications = await _notificationService.GetForUserAsync(userId);
                var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();

                foreach (var notification in unreadNotifications)
                {
                    await _notificationService.MarkAsReadAsync(notification.NotificationId);
                }

                return Ok(new { success = true, message = $"{unreadNotifications.Count} notifications marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error marking all notifications: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create notification (for testing or admin purposes)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    return BadRequest(new { success = false, message = "Title is required" });
                }

                if (string.IsNullOrWhiteSpace(dto.Message))
                {
                    return BadRequest(new { success = false, message = "Message is required" });
                }

                var notification = await _notificationService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = notification.NotificationId },
                    new { success = true, data = notification });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating notification: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete notification by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                // Verify notification belongs to current user
                var notification = await _notificationService.GetByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new { success = false, message = "Notification not found" });
                }

                if (notification.UserId != userId)
                {
                    return Forbid();
                }

                var success = await _notificationService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { success = false, message = "Notification not found or already deleted" });
                }

                return Ok(new { success = true, message = "Notification deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting notification: {ex.Message}" });
            }
        }
    }
}
