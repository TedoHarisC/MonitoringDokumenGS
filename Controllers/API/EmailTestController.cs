using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models;
using MonitoringDokumenGS.Services.Infrastructure;
using System;
using System.Threading.Tasks;

namespace MonitoringDokumenGS.Controllers.API
{
    /// <summary>
    /// Email testing controller
    /// </summary>
    [ApiController]
    [Route("api/test/email")]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailTestController> _logger;
        private readonly EmailOptions _emailOptions;

        public EmailTestController(
            IEmailService emailService,
            ILogger<EmailTestController> logger,
            IOptions<EmailOptions> emailOptions)
        {
            _emailService = emailService;
            _logger = logger;
            _emailOptions = emailOptions.Value;
        }

        /// <summary>
        /// Send test email
        /// POST api/test/email/send
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            try
            {
                var htmlBody = EmailTemplateHelper.GetNotificationEmail(
                    title: "Test Email - ABB Monitoring System",
                    message: "This is a test email from Monitoring Dokumen GS system. If you received this, email configuration is working correctly!",
                    referenceId: "TEST-" + DateTime.Now.Ticks,
                    type: "Test",
                    date: DateTime.Now.ToString("MMMM dd, yyyy HH:mm:ss"),
                    actionLink: request.ActionLink ?? "http://localhost:5170",
                    actionButtonText: "Visit Dashboard",
                    iconBackgroundColor: "#0d6efd",
                    icon: "✉️"
                );

                await _emailService.SendAsync(
                    to: request.Email,
                    subject: "Test Email - ABB Monitoring System",
                    htmlBody: htmlBody
                );

                _logger.LogInformation("Test email sent successfully to {Email}", request.Email);

                return Ok(new
                {
                    success = true,
                    message = $"Test email sent successfully to {request.Email}",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Email}", request.Email);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send email",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Send test invoice email
        /// POST api/test/email/send-invoice
        /// </summary>
        [HttpPost("send-invoice")]
        public async Task<IActionResult> SendTestInvoiceEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            try
            {
                var htmlBody = EmailTemplateHelper.GetInvoiceCreatedEmail(
                    invoiceNumber: "INV-TEST-" + DateTime.Now.ToString("yyyyMMdd"),
                    vendorName: "PT. Test Vendor Indonesia",
                    amount: "Rp 75,500,000",
                    dueDate: DateTime.Now.AddDays(30).ToString("MMMM dd, yyyy"),
                    status: "Pending Approval",
                    createdBy: "Test User",
                    createdDate: DateTime.Now.ToString("MMMM dd, yyyy"),
                    description: "This is a test invoice email. Payment for IT services and consulting for January 2026.",
                    invoiceLink: request.ActionLink ?? "http://localhost:5170/invoice/test"
                );

                await _emailService.SendAsync(
                    to: request.Email,
                    subject: $"Test Invoice - INV-TEST-{DateTime.Now:yyyyMMdd}",
                    htmlBody: htmlBody
                );

                _logger.LogInformation("Test invoice email sent to {Email}", request.Email);

                return Ok(new
                {
                    success = true,
                    message = $"Test invoice email sent to {request.Email}",
                    invoiceNumber = "INV-TEST-" + DateTime.Now.ToString("yyyyMMdd")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test invoice email to {Email}", request.Email);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send invoice email",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Send test contract email
        /// POST api/test/email/send-contract
        /// </summary>
        [HttpPost("send-contract")]
        public async Task<IActionResult> SendTestContractEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            try
            {
                var htmlBody = EmailTemplateHelper.GetContractCreatedEmail(
                    contractNumber: "CON-TEST-" + DateTime.Now.ToString("yyyyMMdd"),
                    vendorName: "PT. Test Supplier Corporation",
                    contractValue: "Rp 350,000,000",
                    startDate: DateTime.Now.ToString("MMMM dd, yyyy"),
                    endDate: DateTime.Now.AddYears(1).ToString("MMMM dd, yyyy"),
                    status: "Active",
                    createdBy: "Test Manager",
                    createdDate: DateTime.Now.ToString("MMMM dd, yyyy"),
                    description: "This is a test contract email. Annual maintenance and support contract for all office equipment.",
                    contractLink: request.ActionLink ?? "http://localhost:5170/contract/test"
                );

                await _emailService.SendAsync(
                    to: request.Email,
                    subject: $"Test Contract - CON-TEST-{DateTime.Now:yyyyMMdd}",
                    htmlBody: htmlBody
                );

                _logger.LogInformation("Test contract email sent to {Email}", request.Email);

                return Ok(new
                {
                    success = true,
                    message = $"Test contract email sent to {request.Email}",
                    contractNumber = "CON-TEST-" + DateTime.Now.ToString("yyyyMMdd")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test contract email to {Email}", request.Email);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send contract email",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Send welcome email
        /// POST api/test/email/send-welcome
        /// </summary>
        [HttpPost("send-welcome")]
        public async Task<IActionResult> SendWelcomeEmail([FromBody] WelcomeEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            try
            {
                var htmlBody = EmailTemplateHelper.GetWelcomeEmail(
                    userName: request.UserName ?? "Test User",
                    username: request.Username ?? "testuser",
                    email: request.Email,
                    role: request.Role ?? "User",
                    loginLink: request.LoginLink ?? "http://localhost:5170/auth/login",
                    supportEmail: "support@abb.com"
                );

                await _emailService.SendAsync(
                    to: request.Email,
                    subject: "Welcome to ABB Monitoring System",
                    htmlBody: htmlBody
                );

                _logger.LogInformation("Welcome email sent to {Email}", request.Email);

                return Ok(new
                {
                    success = true,
                    message = $"Welcome email sent to {request.Email}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", request.Email);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send welcome email",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get SMTP configuration (without sensitive data)
        /// GET api/test/email/config
        /// </summary>
        [HttpGet("config")]
        public IActionResult GetSmtpConfiguration()
        {
            try
            {
                var config = new
                {
                    provider = _emailOptions.Provider ?? "Not Set",
                    fromName = _emailOptions.FromName ?? "Not Set",
                    fromEmail = _emailOptions.FromEmail ?? "Not Set",
                    host = _emailOptions.Smtp?.Host ?? "Not Set",
                    port = _emailOptions.Smtp?.Port ?? 0,
                    useSsl = _emailOptions.Smtp?.UseSsl ?? false,
                    isConfigured = !string.IsNullOrEmpty(_emailOptions.Smtp?.Host)
                };

                return Ok(new
                {
                    success = true,
                    data = config,
                    message = "SMTP configuration retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get SMTP configuration");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve SMTP configuration",
                    error = ex.Message
                });
            }
        }
    }

    public class TestEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? ActionLink { get; set; }
    }

    public class WelcomeEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? LoginLink { get; set; }
    }
}
