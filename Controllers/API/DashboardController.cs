using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDokumenGS.Interfaces;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboard _dashboard;

    public DashboardController(IDashboard dashboard)
    {
        _dashboard = dashboard;
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
}
