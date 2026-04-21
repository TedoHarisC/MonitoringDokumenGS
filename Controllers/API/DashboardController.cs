using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Interfaces;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboard _dashboard;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboard dashboard, ILogger<DashboardController> logger)
    {
        _dashboard = dashboard;
        _logger = logger;
    }

    [HttpGet("budget/{year}")]
    public async Task<IActionResult> GetBudget(int year)
    {
        var data = await _dashboard.GetMonthlyBudgetAsync(year);
        return Ok(data);
    }

    [HttpGet("top-vendors")]
    public async Task<IActionResult> GetTopVendors([FromQuery] int top = 10, [FromQuery] int? year = null)
    {
        var data = await _dashboard.GetTopVendorsAsync(top, year);
        return Ok(data);
    }

    [HttpGet("budget-kpi/{year}")]
    public async Task<IActionResult> GetBudgetKpiByVendor(int year)
    {
        var data = await _dashboard.GetBudgetKpiByVendorAsync(year);
        return Ok(data);
    }

    [HttpGet("budget-summary/{year}")]
    public async Task<IActionResult> GetBudgetSummary(int year)
    {
        var data = await _dashboard.GetBudgetSummaryAsync(year);
        return Ok(data);
    }

    [HttpGet("monthly-realisasi/{year}")]
    public async Task<IActionResult> GetMonthlyRealisasi(int year)
    {
        var data = await _dashboard.GetMonthlyRealisasiAsync(year);
        return Ok(data);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var data = await _dashboard.GetDashboardStatsAsync();
        return Ok(data);
    }

    [HttpGet("vendor-ontime-submission")]
    public async Task<IActionResult> GetVendorOnTimeSubmissionKpi([FromQuery] int? year = null)
    {
        var data = await _dashboard.GetVendorOnTimeSubmissionKpiAsync(year);
        return Ok(data);
    }

    [HttpGet("user-monthly-trend")]
    public async Task<IActionResult> GetUserMonthlyTrend([FromQuery] int? year = null)
    {
        // Get current user ID from claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var data = await _dashboard.GetUserMonthlyInvoiceTrendAsync(userId, year);
        return Ok(data);
    }

    [HttpGet("contracts-expiring")]
    public async Task<IActionResult> GetContractsExpiring([FromQuery] int days = 30)
    {
        try
        {
            _logger.LogInformation("Fetching contracts expiring in {Days} days", days);
            var contracts = await _dashboard.GetContractsExpiringSoonAsync(days);
            _logger.LogInformation("Found {Count} contracts expiring", contracts.Count());
            return Ok(new { success = true, data = contracts, count = contracts.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contracts expiring in {Days} days", days);
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("invoice-status-summary")]
    public async Task<IActionResult> GetInvoiceStatusSummary()
    {
        var data = await _dashboard.GetInvoiceStatusSummaryAsync();
        return Ok(data);
    }
}
