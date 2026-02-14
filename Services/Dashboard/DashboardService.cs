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
        // Get ALL budgets for the year (per vendor category)
        var budgets = await _context.MST_Budget
            .Where(b => b.Year == year)
            .ToListAsync();

        if (!budgets.Any())
        {
            return new List<BudgetKpiDto>();
        }

        // Get realisasi per vendor with category information
        var vendorRealisasi = await _context.Invoices
            .Include(i => i.Vendor)
            .ThenInclude(v => v.VendorCategory)
            .Where(i => !i.IsDeleted && i.InvoiceYear == year && i.Vendor != null)
            .GroupBy(i => new
            {
                i.Vendor.VendorId,
                i.Vendor.VendorName,
                CategoryName = i.Vendor.VendorCategory.Name
            })
            .Select(g => new
            {
                VendorId = g.Key.VendorId,
                VendorName = g.Key.VendorName,
                CategoryName = g.Key.CategoryName,
                Realisasi = g.Sum(i => i.InvoiceAmount)
            })
            .ToListAsync();

        // Create budget lookup by category
        var budgetByCategory = budgets.ToDictionary(
            b => b.TypeBudget ?? "",
            b => b.TotalBudget
        );

        var result = vendorRealisasi.Select(v =>
        {
            // Get budget for this vendor's category
            var categoryBudget = budgetByCategory.ContainsKey(v.CategoryName ?? "")
                ? budgetByCategory[v.CategoryName ?? ""]
                : 0;

            // Count vendors in same category for budget allocation
            var vendorsInCategory = vendorRealisasi.Count(x => x.CategoryName == v.CategoryName);
            var allocatedBudget = vendorsInCategory > 0 && categoryBudget > 0
                ? categoryBudget / vendorsInCategory
                : 0;

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
                VendorName = $"{v.VendorName} ({v.CategoryName})",
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
        // Get ALL budgets for the year (per vendor category)
        var budgets = await _context.MST_Budget
            .Where(b => b.Year == year)
            .ToListAsync();

        // Calculate total budget from all categories
        var totalBudget = budgets.Sum(b => b.TotalBudget);

        if (!budgets.Any())
        {
            var totalRealisasiNobudget = await _context.Invoices
                .Where(i => !i.IsDeleted && i.InvoiceYear == year)
                .SumAsync(i => (decimal?)i.InvoiceAmount) ?? 0;

            return new BudgetSummaryDto
            {
                TotalBudget = 0,
                TotalRealisasi = totalRealisasiNobudget,
                TotalSisaBudget = 0,
                OverallPersentaseSerapan = 0,
                OverallTrafficLight = "green"
            };
        }

        // Calculate realisasi per category
        var categoryRealisasi = new List<decimal>();
        foreach (var budget in budgets)
        {
            var categoryName = budget.TypeBudget?.Trim();

            // Get vendor IDs for this category
            var vendorIds = string.IsNullOrWhiteSpace(categoryName)
                ? await _context.Vendors.Where(v => !v.IsDeleted).Select(v => v.VendorId).ToListAsync()
                : await _context.Vendors
                    .Include(v => v.VendorCategory)
                    .Where(v => !v.IsDeleted && v.VendorCategory.Name == categoryName)
                    .Select(v => v.VendorId)
                    .ToListAsync();

            // Sum invoices for this category
            var spent = await _context.Invoices
                .Where(i => i.InvoiceYear == year && !i.IsDeleted && vendorIds.Contains(i.VendorId))
                .SumAsync(i => (decimal?)i.InvoiceAmount) ?? 0;

            categoryRealisasi.Add(spent);
        }

        var totalRealisasi = categoryRealisasi.Sum();
        var sisaBudget = totalBudget - totalRealisasi;
        var persentaseSerapan = totalBudget > 0 ? (totalRealisasi / totalBudget) * 100 : 0;

        string trafficLight = "green";
        if (persentaseSerapan >= 95)
            trafficLight = "red";
        else if (persentaseSerapan >= 80)
            trafficLight = "yellow";

        return new BudgetSummaryDto
        {
            TotalBudget = totalBudget,
            TotalRealisasi = totalRealisasi,
            TotalSisaBudget = sisaBudget,
            OverallPersentaseSerapan = persentaseSerapan,
            OverallTrafficLight = trafficLight
        };
    }

    public async Task<IEnumerable<MonthlyRealisasiDto>> GetMonthlyRealisasiAsync(int year)
    {
        // Get ALL budgets for the year (per vendor category)
        var budgets = await _context.MST_Budget
            .Where(b => b.Year == year)
            .ToListAsync();

        // Calculate total monthly budget from all categories
        var totalMonthlyBudget = budgets.Sum(b => b.MonthlyBudget);

        // Get monthly realisasi aggregated from all categories
        var monthlyDataByCategory = new Dictionary<int, decimal>();

        foreach (var budget in budgets)
        {
            var categoryName = budget.TypeBudget?.Trim();

            // Get vendor IDs for this category
            var vendorIds = string.IsNullOrWhiteSpace(categoryName)
                ? await _context.Vendors.Where(v => !v.IsDeleted).Select(v => v.VendorId).ToListAsync()
                : await _context.Vendors
                    .Include(v => v.VendorCategory)
                    .Where(v => !v.IsDeleted && v.VendorCategory.Name == categoryName)
                    .Select(v => v.VendorId)
                    .ToListAsync();

            // Get monthly spending for this category
            var categoryMonthly = await _context.Invoices
                .Where(i => !i.IsDeleted && i.InvoiceYear == year && vendorIds.Contains(i.VendorId))
                .GroupBy(i => i.InvoiceMonth)
                .Select(g => new { Month = g.Key, Total = g.Sum(i => i.InvoiceAmount) })
                .ToListAsync();

            // Aggregate to total
            foreach (var item in categoryMonthly)
            {
                if (monthlyDataByCategory.ContainsKey(item.Month))
                    monthlyDataByCategory[item.Month] += item.Total;
                else
                    monthlyDataByCategory[item.Month] = item.Total;
            }
        }

        // Add month names
        var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        // Fill all 12 months
        var allMonths = Enumerable.Range(1, 12).Select(m => new MonthlyRealisasiDto
        {
            Month = m,
            MonthName = monthNames[m],
            Realisasi = monthlyDataByCategory.ContainsKey(m) ? monthlyDataByCategory[m] : 0,
            Budget = totalMonthlyBudget
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

    public async Task<OnTimeSubmissionKpiDto> GetVendorOnTimeSubmissionKpiAsync(int? year = null)
    {
        // Build query for invoices
        var query = _context.Invoices
            .Include(i => i.Vendor)
            .Where(i => !i.IsDeleted && i.Vendor != null);

        // Filter by year if provided
        if (year.HasValue)
        {
            query = query.Where(i => i.InvoiceYear == year.Value);
        }

        // Get all invoices with their on-time status
        var invoices = await query.ToListAsync();

        // Calculate overall KPI
        var totalInvoices = invoices.Count;
        var onTimeInvoices = invoices.Count(i => i.IsOnTime);
        var lateInvoices = totalInvoices - onTimeInvoices;
        var onTimePercentage = totalInvoices > 0 ? (decimal)onTimeInvoices / totalInvoices * 100 : 0;

        // Group by vendor to get vendor breakdown
        var vendorBreakdown = invoices
            .GroupBy(i => new { i.Vendor.VendorId, i.Vendor.VendorName })
            .Select(g =>
            {
                var total = g.Count();
                var onTime = g.Count(i => i.IsOnTime);
                var late = total - onTime;
                var percentage = total > 0 ? (decimal)onTime / total * 100 : 0;

                // Determine performance status
                string performanceStatus;
                if (percentage >= 90)
                    performanceStatus = "Excellent";
                else if (percentage >= 70)
                    performanceStatus = "Good";
                else
                    performanceStatus = "Poor";

                return new VendorOnTimeSubmissionDto
                {
                    VendorName = g.Key.VendorName,
                    TotalInvoices = total,
                    OnTimeSubmissions = onTime,
                    LateSubmissions = late,
                    OnTimePercentage = Math.Round(percentage, 2),
                    PerformanceStatus = performanceStatus
                };
            })
            .OrderByDescending(v => v.OnTimePercentage)
            .ToList();

        return new OnTimeSubmissionKpiDto
        {
            TotalInvoices = totalInvoices,
            OnTimeSubmissions = onTimeInvoices,
            LateSubmissions = lateInvoices,
            OnTimePercentage = Math.Round(onTimePercentage, 2),
            VendorBreakdown = vendorBreakdown
        };
    }

    public async Task<object> GetUserMonthlyInvoiceTrendAsync(Guid userId, int? year = null)
    {
        // Get user's vendor
        var user = await _context.Users
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync();

        if (user == null || user.VendorId == Guid.Empty)
        {
            return new
            {
                months = new string[] { },
                onTimeData = new int[] { },
                lateData = new int[] { }
            };
        }

        // Build query for user's vendor invoices
        var query = _context.Invoices
            .Where(i => !i.IsDeleted && i.VendorId == user.VendorId);

        // Filter by year if provided
        if (year.HasValue)
        {
            query = query.Where(i => i.InvoiceYear == year.Value);
        }

        var invoices = await query.ToListAsync();

        // Group by month and calculate on-time vs late
        var monthlyData = invoices
            .GroupBy(i => i.InvoiceMonth)
            .Select(g => new
            {
                Month = g.Key,
                OnTime = g.Count(i => i.IsOnTime),
                Late = g.Count(i => !i.IsOnTime)
            })
            .OrderBy(x => x.Month)
            .ToList();

        // Fill all 12 months with data (0 if no data)
        var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var onTimeData = new int[12];
        var lateData = new int[12];

        foreach (var data in monthlyData)
        {
            if (data.Month >= 1 && data.Month <= 12)
            {
                onTimeData[data.Month - 1] = data.OnTime;
                lateData[data.Month - 1] = data.Late;
            }
        }

        return new
        {
            months = monthNames,
            onTimeData = onTimeData,
            lateData = lateData
        };
    }

    public async Task<IEnumerable<ContractExpiringDto>> GetContractsExpiringSoonAsync(int days = 30)
    {
        var today = DateTime.Now.Date;
        var futureDate = today.AddDays(days);

        var contracts = await _context.Contracts
            .Include(c => c.Vendor)
            .Include(c => c.ContractStatus)
            .Where(c => !c.IsDeleted && c.EndDate >= today && c.EndDate <= futureDate)
            .OrderBy(c => c.EndDate)
            .Select(c => new ContractExpiringDto
            {
                ContractId = c.ContractId,
                ContractNo = c.ContractNumber,
                VendorName = c.Vendor != null ? c.Vendor.VendorName : "N/A",
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                DaysRemaining = (int)(c.EndDate.Date - today).TotalDays,
                ContractValue = 0, // Contract model doesn't have TotalValue
                Status = c.ContractStatus != null ? c.ContractStatus.Name : "N/A",
                AlertLevel = (int)(c.EndDate.Date - today).TotalDays < 15 ? "Critical" :
                             (int)(c.EndDate.Date - today).TotalDays < 30 ? "Warning" : "Safe",
                Description = c.ContractDescription ?? ""
            })
            .ToListAsync();

        return contracts;
    }
}
