using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;

public class DashboardService : IDashboard
{
    private readonly ApplicationDBContext _context;
    private readonly IAuditLog _auditLog;

    public DashboardService(ApplicationDBContext context, IAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<IEnumerable<DashboardBudgetMonthlyDto>> GetMonthlyBudgetAsync(int year)
    {
        var sql = @"
            SELECT *
            FROM V_Dashboard_Budget_Monthly
            WHERE Year = @year
            ORDER BY Month";

        return await _context.V_Dashboard_Budget_Monthly
            .FromSqlRaw(sql, new Microsoft.Data.SqlClient.SqlParameter("@year", year))
            .ToListAsync();
    }
}
