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

    public async Task<IEnumerable<TopVendorSpendDto>> GetTopVendorsAsync(int top = 10, int? year = null)
    {
        var query = _context.Invoices
            .Where(i => !i.IsDeleted);

        // Filter by year if provided
        if (year.HasValue)
        {
            query = query.Where(i => i.InvoiceYear == year.Value);
        }

        var topVendors = await query
            .GroupBy(i => new { i.Vendor!.VendorId, i.Vendor.VendorName })
            .Select(g => new TopVendorSpendDto
            {
                VendorName = g.Key.VendorName,
                TotalSpend = g.Sum(i => i.InvoiceAmount)
            })
            .OrderByDescending(v => v.TotalSpend)
            .Take(top)
            .ToListAsync();

        return topVendors;
    }
}
