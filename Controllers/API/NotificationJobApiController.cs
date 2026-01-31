using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Interfaces;

[Authorize(Roles = "SUPER_ADMIN,ADMIN")]
[ApiController]
[Route("api/jobs")]
public class NotificationJobApiController : ControllerBase
{
    private readonly IContractNotificationJob _contractJob;
    private readonly IBudgetNotificationJob _budgetJob;
    private readonly ILogger<NotificationJobApiController> _logger;
    private readonly IConfiguration _configuration;

    public NotificationJobApiController(
        IContractNotificationJob contractJob,
        IBudgetNotificationJob budgetJob,
        ILogger<NotificationJobApiController> logger,
        IConfiguration configuration)
    {
        _contractJob = contractJob;
        _budgetJob = budgetJob;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Run contract expiry notification job manually.
    /// </summary>
    [HttpPost("contract-expiry")]
    public async Task<IActionResult> RunContractExpiry()
    {
        try
        {
            await _contractJob.RunAsync();
            return Ok(new
            {
                success = true,
                message = "Contract expiry notification job executed successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running contract expiry job");

            return StatusCode(500, new
            {
                success = false,
                message = "Failed to execute contract expiry notification job."
            });
        }
    }

    /// <summary>
    /// Run budget overbudget check job manually.
    /// Checks for overbudget conditions and sends alerts to admins.
    /// </summary>
    [HttpPost("budget-overbudget")]
    public async Task<IActionResult> RunBudgetOverbudgetCheck()
    {
        try
        {
            await _budgetJob.RunAsync();
            return Ok(new
            {
                success = true,
                message = "Budget overbudget check executed successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running budget overbudget check");

            return StatusCode(500, new
            {
                success = false,
                message = "Failed to execute budget overbudget check.",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get current budget status for a specific year.
    /// Shows budget utilization, overbudget status, and monthly breakdown.
    /// </summary>
    [HttpGet("budget-status/{year}")]
    public async Task<IActionResult> GetBudgetStatus(int year)
    {
        try
        {
            var status = await _budgetJob.GetBudgetStatusAsync(year);

            if (status == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No budget configured for year {year}"
                });
            }

            return Ok(new
            {
                success = true,
                data = status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget status for year {Year}", year);

            return StatusCode(500, new
            {
                success = false,
                message = "Failed to retrieve budget status.",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Test budget alert by sending to a specific email address.
    /// Simulates overbudget scenario for testing purposes.
    /// </summary>
    [HttpPost("budget-alert-test")]
    public async Task<IActionResult> TestBudgetAlert([FromBody] TestBudgetAlertRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required" });
            }

            var year = request.Year ?? DateTime.Now.Year;
            var status = await _budgetJob.GetBudgetStatusAsync(year);

            if (status == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No budget configured for year {year}"
                });
            }

            // Use actual data or mock data for testing
            var testData = new
            {
                Type = request.Type ?? "MONTHLY",
                Year = year,
                Month = request.Month ?? DateTime.Now.Month,
                Budget = request.Budget ?? status.MonthlyBudget,
                Spent = request.Spent ?? (status.MonthlyBudget * 1.25m), // 125% of budget for demo
                Period = request.Type == "YEARLY"
                    ? $"Year {year}"
                    : $"{GetMonthName(request.Month ?? DateTime.Now.Month)} {year}"
            };

            var excess = testData.Spent - testData.Budget;
            var utilizationPercent = testData.Budget > 0 ? (testData.Spent / testData.Budget) * 100 : 0;
            var excessPercent = utilizationPercent - 100;

            var appUrl = _configuration["AppUrl"] ?? "http://localhost:5008";
            var title = $"⚠️ BUDGET OVERRUN ALERT (TEST) - {testData.Period}";
            var emailBody = MonitoringDokumenGS.Services.Infrastructure.EmailTemplateHelper.GetNotificationEmail(
                title: title,
                message: $@"This is a TEST alert. Budget has been exceeded for {testData.Period}.<br/><br/>
                           <strong>Test Data:</strong><br/>
                           Budget: {testData.Budget:N2}<br/>
                           Spent: {testData.Spent:N2}<br/>
                           Over Budget: {excess:N2} ({excessPercent:N1}%)<br/>
                           Utilization: {utilizationPercent:N1}%",
                referenceId: $"TEST-BUDGET-{testData.Type}-{testData.Year}-{testData.Month}",
                type: "Budget Alert Test",
                date: DateTime.Now.ToString("MMMM dd, yyyy HH:mm"),
                actionLink: $"{appUrl}/Master/Budget",
                actionButtonText: "View Budget Details",
                iconBackgroundColor: "#dc3545",
                icon: "⚠️"
            );

            // Get email service from DI
            var emailService = HttpContext.RequestServices.GetRequiredService<MonitoringDokumenGS.Interfaces.IEmailService>();

            await emailService.SendAsync(
                to: request.Email,
                subject: title,
                htmlBody: emailBody
            );

            _logger.LogInformation("Test budget alert sent to {Email}", request.Email);

            return Ok(new
            {
                success = true,
                message = $"Test budget alert sent to {request.Email}",
                testData = new
                {
                    period = testData.Period,
                    budget = testData.Budget,
                    spent = testData.Spent,
                    overBudget = excess,
                    utilizationPercent = utilizationPercent,
                    recipient = request.Email
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test budget alert to {Email}", request.Email);

            return StatusCode(500, new
            {
                success = false,
                message = "Failed to send test budget alert.",
                error = ex.Message
            });
        }
    }

    private string GetMonthName(int month)
    {
        var months = new[] { "", "January", "February", "March", "April", "May", "June",
                            "July", "August", "September", "October", "November", "December" };
        return month >= 1 && month <= 12 ? months[month] : "Unknown";
    }
}

public class TestBudgetAlertRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Type { get; set; } = "MONTHLY"; // "MONTHLY" or "YEARLY"
    public int? Year { get; set; }
    public int? Month { get; set; }
    public decimal? Budget { get; set; }
    public decimal? Spent { get; set; }
}
