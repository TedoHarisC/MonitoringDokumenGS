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
}
