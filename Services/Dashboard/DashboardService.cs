using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos.Dashboard;
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
            .Include(i => i.Vendor)
            .Where(i => !i.IsDeleted && i.Vendor != null);

        // Filter by year if provided
        if (year.HasValue)
        {
            query = query.Where(i => i.InvoiceYear == year.Value);
        }

        var topVendors = await query
            .GroupBy(i => new { i.Vendor.VendorId, i.Vendor.VendorName })
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

    public async Task<IEnumerable<BudgetKpiDto>> GetBudgetKpiByVendorAsync(int year)
    {
        // Get budget for the year
        var budget = await _context.MST_Budget
            .Where(b => b.Year == year)
            .FirstOrDefaultAsync();

        if (budget == null)
        {
            return new List<BudgetKpiDto>();
        }

        // Get realisasi per vendor from invoices
        var vendorRealisasi = await _context.Invoices
            .Include(i => i.Vendor)
            .Where(i => !i.IsDeleted && i.InvoiceYear == year && i.Vendor != null)
            .GroupBy(i => new { i.Vendor.VendorId, i.Vendor.VendorName })
            .Select(g => new
            {
                VendorId = g.Key.VendorId,
                VendorName = g.Key.VendorName,
                Realisasi = g.Sum(i => i.InvoiceAmount)
            })
            .ToListAsync();

        // Get total contracts per vendor to calculate budget allocation
        var totalContracts = await _context.Contracts
            .Where(c => !c.IsDeleted)
            .CountAsync();

        var result = vendorRealisasi.Select(v =>
        {
            // Simple budget allocation: divide total budget by number of vendors
            var allocatedBudget = vendorRealisasi.Count > 0 ? budget.TotalBudget / vendorRealisasi.Count : 0;
            var sisaBudget = allocatedBudget - v.Realisasi;
            var persentaseSerapan = allocatedBudget > 0 ? (v.Realisasi / allocatedBudget) * 100 : 0;
            var variance = allocatedBudget - v.Realisasi;
            var variancePercentage = allocatedBudget > 0 ? (variance / allocatedBudget) * 100 : 0;

            // Traffic light logic
            string trafficLight = "green";
            if (persentaseSerapan >= 95)
                trafficLight = "red";
            else if (persentaseSerapan >= 80)
                trafficLight = "yellow";

            return new BudgetKpiDto
            {
                VendorName = v.VendorName,
                TotalBudget = allocatedBudget,
                Realisasi = v.Realisasi,
                SisaBudget = sisaBudget,
                PersentaseSerapan = persentaseSerapan,
                Variance = variance,
                VariancePercentage = variancePercentage,
                TrafficLight = trafficLight
            };
        }).OrderByDescending(x => x.Realisasi).ToList();

        return result;
    }

    public async Task<BudgetSummaryDto> GetBudgetSummaryAsync(int year)
    {
        var budget = await _context.MST_Budget
            .Where(b => b.Year == year)
            .FirstOrDefaultAsync();

        var totalRealisasi = await _context.Invoices
            .Where(i => !i.IsDeleted && i.InvoiceYear == year)
            .SumAsync(i => (decimal?)i.InvoiceAmount) ?? 0;

        if (budget == null)
        {
            return new BudgetSummaryDto
            {
                TotalBudget = 0,
                TotalRealisasi = totalRealisasi,
                TotalSisaBudget = 0,
                OverallPersentaseSerapan = 0,
                OverallTrafficLight = "green"
            };
        }

        var sisaBudget = budget.TotalBudget - totalRealisasi;
        var persentaseSerapan = budget.TotalBudget > 0 ? (totalRealisasi / budget.TotalBudget) * 100 : 0;

        string trafficLight = "green";
        if (persentaseSerapan >= 95)
            trafficLight = "red";
        else if (persentaseSerapan >= 80)
            trafficLight = "yellow";

        return new BudgetSummaryDto
        {
            TotalBudget = budget.TotalBudget,
            TotalRealisasi = totalRealisasi,
            TotalSisaBudget = sisaBudget,
            OverallPersentaseSerapan = persentaseSerapan,
            OverallTrafficLight = trafficLight
        };
    }

    public async Task<IEnumerable<MonthlyRealisasiDto>> GetMonthlyRealisasiAsync(int year)
    {
        var budget = await _context.MST_Budget
            .Where(b => b.Year == year)
            .FirstOrDefaultAsync();

        var monthlyBudget = budget != null ? budget.MonthlyBudget : 0;

        var monthlyData = await _context.Invoices
            .Where(i => !i.IsDeleted && i.InvoiceYear == year)
            .GroupBy(i => i.InvoiceMonth)
            .Select(g => new MonthlyRealisasiDto
            {
                Month = g.Key,
                MonthName = "",
                Realisasi = g.Sum(i => i.InvoiceAmount),
                Budget = monthlyBudget
            })
            .ToListAsync();

        // Add month names
        var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        foreach (var item in monthlyData)
        {
            item.MonthName = monthNames[item.Month];
        }

        // Fill missing months with zero
        var allMonths = Enumerable.Range(1, 12).Select(m => new MonthlyRealisasiDto
        {
            Month = m,
            MonthName = monthNames[m],
            Realisasi = monthlyData.FirstOrDefault(x => x.Month == m)?.Realisasi ?? 0,
            Budget = monthlyBudget
        }).ToList();

        return allMonths;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.Now;
        var next30Days = today.AddDays(30);

        // Count active contracts (between StartDate and EndDate)
        var activeContractsCount = await _context.Contracts
            .Where(c => !c.IsDeleted && c.StartDate <= today && c.EndDate >= today)
            .CountAsync();

        // Count contracts expiring in next 30 days
        var contractsExpiringSoon = await _context.Contracts
            .Where(c => !c.IsDeleted && c.EndDate >= today && c.EndDate <= next30Days)
            .CountAsync();

        // Count total invoices submitted
        var totalInvoicesSubmitted = await _context.Invoices
            .Where(i => !i.IsDeleted)
            .CountAsync();

        // Sum total invoice amount
        var totalInvoiceAmount = await _context.Invoices
            .Where(i => !i.IsDeleted)
            .SumAsync(i => i.InvoiceAmount);

        return new DashboardStatsDto
        {
            ActiveContractsCount = activeContractsCount,
            TotalInvoicesSubmitted = totalInvoicesSubmitted,
            TotalInvoiceAmount = totalInvoiceAmount,
            ContractsExpiringSoon = contractsExpiringSoon
        };
    }
}
