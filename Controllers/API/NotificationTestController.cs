using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Services.Infrastructure;

namespace MonitoringDokumenGS.Controllers.API
{
    [ApiController]
    [Route("api/test/notifications")]
    public class NotificationTestController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationTestController> _logger;
        private readonly IBudgetNotificationJob _budgetJob;
        private readonly IContractNotificationJob _contractJob;

        public NotificationTestController(
            ApplicationDBContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<NotificationTestController> logger,
            IBudgetNotificationJob budgetJob,
            IContractNotificationJob contractJob)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _budgetJob = budgetJob;
            _contractJob = contractJob;
        }

        /// <summary>
        /// Preview Budget Overbudget Email Template (HTML)
        /// </summary>
        [HttpGet("preview/budget-alert")]
        public IActionResult PreviewBudgetAlert()
        {
            var appUrl = _configuration["AppUrl"] ?? "http://localhost:5008";

            var emailHtml = EmailTemplateHelper.GetNotificationEmail(
                title: "⚠️ BUDGET OVERRUN ALERT - January 2026",
                message: "Budget has been exceeded for January 2026.",
                referenceId: "BUDGET-MONTHLY-2026-01",
                type: "Budget Alert",
                date: DateTime.Now.ToString("MMMM dd, yyyy HH:mm"),
                actionLink: $"{appUrl}/Master/Budget",
                actionButtonText: "View Budget Details",
                iconBackgroundColor: "#dc3545",
                icon: "⚠️"
            );

            return Content(emailHtml, "text/html");
        }

        /// <summary>
        /// Preview Contract Expiring Email Template (HTML)
        /// </summary>
        [HttpGet("preview/contract-expiring")]
        public IActionResult PreviewContractExpiring([FromQuery] int daysLeft = 7)
        {
            var appUrl = _configuration["AppUrl"] ?? "http://localhost:5008";

            var emailHtml = EmailTemplateHelper.GetContractExpiringEmail(
                contractNumber: "CNT-2024-001",
                vendorName: "PT. ABC Indonesia",
                endDate: DateTime.Now.AddDays(daysLeft).ToString("MMMM dd, yyyy"),
                daysLeft: daysLeft.ToString(),
                actionLink: $"{appUrl}/Contract/Detail/123",
                language: "en"
            );

            return Content(emailHtml, "text/html");
        }

        /// <summary>
        /// Send Test Budget Alert Email to Specific Email
        /// </summary>
        [HttpPost("send/budget-alert")]
        public async Task<IActionResult> SendTestBudgetAlert([FromBody] TestEmailRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { success = false, message = "Email is required" });
                }

                var appUrl = _configuration["AppUrl"] ?? "http://localhost:5008";

                var emailHtml = EmailTemplateHelper.GetNotificationEmail(
                    title: "⚠️ TEST: BUDGET OVERRUN ALERT - January 2026",
                    message: "This is a TEST notification. Budget has been exceeded for January 2026.",
                    referenceId: "TEST-BUDGET-MONTHLY-2026-01",
                    type: "Budget Alert (TEST)",
                    date: DateTime.Now.ToString("MMMM dd, yyyy HH:mm"),
                    actionLink: $"{appUrl}/Master/Budget",
                    actionButtonText: "View Budget Details",
                    iconBackgroundColor: "#dc3545",
                    icon: "⚠️"
                );

                await _emailService.SendAsync(
                    to: request.Email,
                    subject: "TEST: Budget Overrun Alert - January 2026",
                    htmlBody: emailHtml
                );

                return Ok(new
                {
                    success = true,
                    message = $"Test budget alert email sent to {request.Email}",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test budget alert email");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send test email",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Send Test Contract Expiring Email to Specific Email
        /// </summary>
        [HttpPost("send/contract-expiring")]
        public async Task<IActionResult> SendTestContractExpiring([FromBody] TestContractEmailRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { success = false, message = "Email is required" });
                }

                var daysLeft = request.DaysLeft ?? 7;
                var appUrl = _configuration["AppUrl"] ?? "http://localhost:5008";

                var emailHtml = EmailTemplateHelper.GetContractExpiringEmail(
                    contractNumber: "CNT-TEST-001",
                    vendorName: "PT. Test Vendor Indonesia",
                    endDate: DateTime.Now.AddDays(daysLeft).ToString("MMMM dd, yyyy"),
                    daysLeft: daysLeft.ToString(),
                    actionLink: $"{appUrl}/Contract",
                    language: "en"
                );

                var urgency = daysLeft <= 3 ? "CRITICAL" : daysLeft <= 7 ? "URGENT" : daysLeft <= 14 ? "HIGH" : "MEDIUM";

                await _emailService.SendAsync(
                    to: request.Email,
                    subject: $"TEST: {urgency} - Contract Expires in {daysLeft} Days",
                    htmlBody: emailHtml
                );

                return Ok(new
                {
                    success = true,
                    message = $"Test contract expiring email sent to {request.Email}",
                    daysLeft = daysLeft,
                    urgency = urgency,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test contract expiring email");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send test email",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Trigger Budget Check (Real Data, Real Email) - Use with caution!
        /// </summary>
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        [HttpPost("trigger/budget-check")]
        public async Task<IActionResult> TriggerBudgetCheck()
        {
            try
            {
                await _budgetJob.RunAsync();

                return Ok(new
                {
                    success = true,
                    message = "Budget check executed with real data. Notifications sent if overbudget detected.",
                    timestamp = DateTime.Now,
                    warning = "This sends REAL emails to admins if budget is overbudget!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering budget check");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to execute budget check",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Trigger Contract Check (Real Data, Real Email) - Use with caution!
        /// </summary>
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        [HttpPost("trigger/contract-check")]
        public async Task<IActionResult> TriggerContractCheck()
        {
            try
            {
                await _contractJob.RunAsync();

                return Ok(new
                {
                    success = true,
                    message = "Contract check executed with real data. Notifications sent for expiring contracts.",
                    timestamp = DateTime.Now,
                    warning = "This sends REAL emails to PICs and admins!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering contract check");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to execute contract check",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get Statistics: Budget Status & Expiring Contracts
        /// </summary>
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetNotificationStats()
        {
            try
            {
                var today = DateTime.Now.Date;
                var year = DateTime.Now.Year;

                // Budget stats
                var budgetStatus = await _budgetJob.GetBudgetStatusAsync(year);

                // Contract stats
                var contractsExpiring60 = await _context.Contracts
                    .Where(c => !c.IsDeleted && c.EndDate >= today && EF.Functions.DateDiffDay(today, c.EndDate) <= 60)
                    .CountAsync();

                var contractsExpiring30 = await _context.Contracts
                    .Where(c => !c.IsDeleted && c.EndDate >= today && EF.Functions.DateDiffDay(today, c.EndDate) <= 30)
                    .CountAsync();

                var contractsExpiring7 = await _context.Contracts
                    .Where(c => !c.IsDeleted && c.EndDate >= today && EF.Functions.DateDiffDay(today, c.EndDate) <= 7)
                    .CountAsync();

                return Ok(new
                {
                    success = true,
                    timestamp = DateTime.Now,
                    budget = new
                    {
                        year = budgetStatus?.Year,
                        isOverbudget = budgetStatus?.IsOverBudget,
                        utilizationPercent = budgetStatus?.BudgetUtilizationPercent,
                        totalBudget = budgetStatus?.TotalBudget,
                        totalSpent = budgetStatus?.TotalSpent,
                        remaining = budgetStatus?.RemainingBudget
                    },
                    contracts = new
                    {
                        expiring60Days = contractsExpiring60,
                        expiring30Days = contractsExpiring30,
                        expiring7Days = contractsExpiring7
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get stats",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// View Notification Log History
        /// </summary>
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        [HttpGet("logs")]
        public async Task<IActionResult> GetNotificationLogs([FromQuery] int limit = 50)
        {
            try
            {
                var logs = await _context.SYS_NotificationLog
                    .Where(n => !n.IsDeleted)
                    .OrderByDescending(n => n.SentAt)
                    .Take(limit)
                    .Select(n => new
                    {
                        n.Id,
                        n.NotificationType,
                        n.ReferenceId,
                        n.RecipientEmail,
                        n.ReminderLevel,
                        n.SentAt,
                        n.Details
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    count = logs.Count,
                    logs = logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification logs");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to get logs",
                    error = ex.Message
                });
            }
        }
    }

    public class TestContractEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public int? DaysLeft { get; set; }
    }
}
