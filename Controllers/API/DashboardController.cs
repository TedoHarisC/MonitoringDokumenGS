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
}
