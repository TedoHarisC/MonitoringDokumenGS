using MonitoringDokumenGS.Dtos.Dashboard;
using MonitoringDokumenGS.Models.Transaction;
using MonitoringDokumenGS.Models.Master;
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

    public async Task<IEnumerable<BudgetCoaPerformanceDto>> GetBudgetCoaPerformanceAsync(
        int year,
        DateTime? startDate = null,
        DateTime? endDate = null,
        List<Guid>? budgetCodeIds = null,
        List<int>? coaTextIds = null)
    {
        // PLAN from master budget, grouped by COA + BudgetCode
        var planQuery =
            from b in _context.MST_Budget
            join vc in _context.VendorCategories on b.BudgetCodeId equals vc.ParentBudgetCodeId
            join bc in _context.BudgetCode on b.BudgetCodeId equals bc.BudgetCodeId
            where b.Year == year
            where budgetCodeIds == null || (b.BudgetCodeId.HasValue && budgetCodeIds.Contains(b.BudgetCodeId.Value))
            where coaTextIds == null || coaTextIds.Contains(vc.VendorCategoryId)
            where (b.NoCoa != null && vc.NoCoa != null && b.NoCoa == vc.NoCoa)
                || (b.TypeBudget != null && vc.Name != null && b.TypeBudget == vc.Name)
            group b by new { vc.VendorCategoryId, vc.Name, bc.BudgetCodeId, bc.Code, bc.Description } into g
            select new
            {
                BudgetCodeId = g.Key.BudgetCodeId,
                BudgetCode = g.Key.Code,
                BudgetCodeDescription = g.Key.Description,
                CoaTextId = g.Key.VendorCategoryId,
                CoaText = g.Key.Name,
                Plan = g.Sum(x => x.TotalBudget)
            };

        var planData = await planQuery.ToListAsync();

        var invoiceQuery = _context.Invoices
            .Include(i => i.Coa)
            .Include(i => i.Budget)
            .ThenInclude(b => b!.BudgetCode)
            .Where(i => !i.IsDeleted && i.InvoiceYear == year && i.CoaTextId.HasValue);

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            invoiceQuery = invoiceQuery.Where(i => i.CreatedAt.Date >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date;
            invoiceQuery = invoiceQuery.Where(i => i.CreatedAt.Date <= end);
        }

        if (budgetCodeIds != null && budgetCodeIds.Count > 0)
        {
            invoiceQuery = invoiceQuery.Where(i => i.BudgetCodeId.HasValue && budgetCodeIds.Contains(i.BudgetCodeId.Value));
        }

        if (coaTextIds != null && coaTextIds.Count > 0)
        {
            invoiceQuery = invoiceQuery.Where(i => i.CoaTextId.HasValue && coaTextIds.Contains(i.CoaTextId.Value));
        }

        // ACTUAL from invoice spending by matched COA + BudgetCode
        var actualData = await invoiceQuery
            .GroupBy(i => new
            {
                BudgetCodeId = i.BudgetCodeId,
                BudgetCode = i.Budget != null && i.Budget.BudgetCode != null ? i.Budget.BudgetCode.Code : string.Empty,
                BudgetCodeDescription = i.Budget != null && i.Budget.BudgetCode != null ? i.Budget.BudgetCode.Description : string.Empty,
                i.CoaTextId,
                CoaText = i.Coa != null ? i.Coa.Name : string.Empty
            })
            .Select(g => new
            {
                BudgetCodeId = g.Key.BudgetCodeId,
                BudgetCode = g.Key.BudgetCode,
                BudgetCodeDescription = g.Key.BudgetCodeDescription,
                CoaTextId = g.Key.CoaTextId ?? 0,
                CoaText = g.Key.CoaText,
                Actual = g.Sum(i => i.GrandTotal ?? (i.InvoiceAmount + i.TaxAmount))
            })
            .ToListAsync();

        // Fallback map: ensure BudgetCode text can still be resolved from BudgetCodeId
        // even when invoice navigation properties are not populated.
        var allBudgetCodeIds = planData
            .Select(x => (Guid?)x.BudgetCodeId)
            .Concat(actualData.Select(x => x.BudgetCodeId))
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        var budgetCodeLookup = !allBudgetCodeIds.Any()
            ? new Dictionary<Guid, string>()
            : await _context.BudgetCode
                .Where(x => allBudgetCodeIds.Contains(x.BudgetCodeId))
                .Select(x => new { x.BudgetCodeId, x.Code, x.Description })
                .ToDictionaryAsync(
                    x => x.BudgetCodeId,
                    x => FormatBudgetCodeLabel(x.Code, x.Description));

        static string FormatBudgetCodeLabel(string? code, string? description)
        {
            var normalizedCode = string.IsNullOrWhiteSpace(code) ? string.Empty : code.Trim();
            var normalizedDescription = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();

            if (!string.IsNullOrEmpty(normalizedCode) && !string.IsNullOrEmpty(normalizedDescription))
            {
                return $"{normalizedCode} - {normalizedDescription}";
            }

            if (!string.IsNullOrEmpty(normalizedCode))
            {
                return normalizedCode;
            }

            return normalizedDescription;
        }

        static string ResolveBudgetCode(params string?[] candidates)
        {
            foreach (var value in candidates)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return "No Budget Code";
        }

        string BuildKey(Guid? budgetId, int coaId) => $"{(budgetId.HasValue ? budgetId.Value.ToString() : "none")}|{coaId}";

        var allKeys = planData
            .Select(p => BuildKey(p.BudgetCodeId, p.CoaTextId))
            .Union(actualData.Select(a => BuildKey(a.BudgetCodeId, a.CoaTextId)))
            .Distinct()
            .ToList();

        var result = allKeys.Select(key =>
        {
            var p = planData.FirstOrDefault(x => BuildKey(x.BudgetCodeId, x.CoaTextId) == key);
            var a = actualData.FirstOrDefault(x => BuildKey(x.BudgetCodeId, x.CoaTextId) == key);

            var plan = p?.Plan ?? 0;
            var actual = a?.Actual ?? 0;
            var variance = actual - plan;
            var utilization = plan > 0 ? (actual / plan) * 100 : 0;

            var coaText = p?.CoaText ?? a?.CoaText ?? "COA";
            var budgetCodeId = p?.BudgetCodeId ?? a?.BudgetCodeId;
            budgetCodeLookup.TryGetValue(budgetCodeId ?? Guid.Empty, out var budgetCodeFromLookup);
            var budgetCode = ResolveBudgetCode(
                FormatBudgetCodeLabel(p?.BudgetCode, p?.BudgetCodeDescription),
                FormatBudgetCodeLabel(a?.BudgetCode, a?.BudgetCodeDescription),
                budgetCodeFromLookup);

            return new BudgetCoaPerformanceDto
            {
                BudgetCodeId = budgetCodeId,
                BudgetCode = budgetCode,
                CoaTextId = p?.CoaTextId ?? a?.CoaTextId ?? 0,
                CoaText = coaText,
                DisplayLabel = $"{coaText} ({budgetCode})",
                Plan = plan,
                Actual = actual,
                Variance = variance,
                UtilizationPercentage = utilization
            };
        })
        .OrderByDescending(x => x.Actual)
        .ThenBy(x => x.BudgetCode)
        .ThenBy(x => x.CoaText)
        .ToList();

        return result;
    }

    public async Task<BudgetSummaryDto> GetBudgetSummaryAsync(int year, Guid? budgetCodeId = null, int? vendorCategoryId = null)
    {
        // When filters are provided, use a direct filtered query
        if (budgetCodeId.HasValue || vendorCategoryId.HasValue)
        {
            // Determine effective budgetCodeId from vendorCategory if not explicitly set
            Guid? effectiveBudgetCodeId = budgetCodeId;
            if (!effectiveBudgetCodeId.HasValue && vendorCategoryId.HasValue)
            {
                effectiveBudgetCodeId = await _context.VendorCategories
                    .Where(vc => vc.VendorCategoryId == vendorCategoryId.Value)
                    .Select(vc => vc.ParentBudgetCodeId)
                    .FirstOrDefaultAsync();
            }

            // Budget total
            var budgetQuery = _context.MST_Budget.Where(b => b.Year == year);
            if (effectiveBudgetCodeId.HasValue)
                budgetQuery = budgetQuery.Where(b => b.BudgetCodeId == effectiveBudgetCodeId.Value);
            var totalBudget = await budgetQuery.SumAsync(b => (decimal?)b.TotalBudget) ?? 0;

            // Realisasi (invoices)
            var invoiceQuery = _context.Invoices.Where(i => !i.IsDeleted && i.InvoiceYear == year);
            if (effectiveBudgetCodeId.HasValue)
                invoiceQuery = invoiceQuery.Where(i => i.BudgetCodeId == effectiveBudgetCodeId.Value);
            if (vendorCategoryId.HasValue)
            {
                var vendorIds = await _context.Vendors
                    .Where(v => !v.IsDeleted && v.VendorCategoryId == vendorCategoryId.Value)
                    .Select(v => v.VendorId)
                    .ToListAsync();
                invoiceQuery = invoiceQuery.Where(i => vendorIds.Contains(i.VendorId));
            }
            var totalRealisasi = await invoiceQuery.SumAsync(i => (decimal?)i.InvoiceAmount) ?? 0;

            var sisa = totalBudget - totalRealisasi;
            var persen = totalBudget > 0 ? (totalRealisasi / totalBudget) * 100 : 0;
            string tl = persen >= 95 ? "red" : persen >= 80 ? "yellow" : "green";

            return new BudgetSummaryDto
            {
                TotalBudget = totalBudget,
                TotalRealisasi = totalRealisasi,
                TotalSisaBudget = sisa,
                OverallPersentaseSerapan = persen,
                OverallTrafficLight = tl
            };
        }

        // Get ALL budgets for the year (per vendor category)
        var budgets = await _context.MST_Budget
            .Where(b => b.Year == year)
            .ToListAsync();

        // Calculate total budget from all categories
        var allBudgetTotal = budgets.Sum(b => b.TotalBudget);

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

        var allRealisasiTotal = categoryRealisasi.Sum();
        var sisaBudget = allBudgetTotal - allRealisasiTotal;
        var persentaseSerapan = allBudgetTotal > 0 ? (allRealisasiTotal / allBudgetTotal) * 100 : 0;

        string trafficLight = "green";
        if (persentaseSerapan >= 95)
            trafficLight = "red";
        else if (persentaseSerapan >= 80)
            trafficLight = "yellow";

        return new BudgetSummaryDto
        {
            TotalBudget = allBudgetTotal,
            TotalRealisasi = allRealisasiTotal,
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
        var today = DateTime.Now.Date;
        var next30Days = today.AddDays(30);

        // Count active contracts (tidak overlap expiring soon, date only)
        var activeContractsCount = await _context.Contracts
            .Where(c => !c.IsDeleted && c.StartDate.Date <= today && c.EndDate.Date > next30Days)
            .CountAsync();

        // Count contracts expiring in next 30 days (tidak overlap active, date only)
        var contractsExpiringSoon = await _context.Contracts
            .Where(c => !c.IsDeleted && c.EndDate.Date >= today && c.EndDate.Date <= next30Days)
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

    public async Task<InvoiceStatusSummaryDto> GetInvoiceStatusSummaryAsync()
    {
        // Ambil semua status dari master
        var allStatuses = await _context.InvoiceProgressStatuses
            .Where(i => !i.IsDeleted)
            .OrderBy(s => s.ProgressStatusId)
            .ToListAsync();

        // Hitung total invoice per status
        var invoiceCounts = await _context.Invoices
            .Where(i => !i.IsDeleted)
            .GroupBy(i => i.ProgressStatusId)
            .Select(g => new { ProgressStatusId = g.Key, Total = g.Count() })
            .ToListAsync();

        // Gabungkan, pastikan semua status muncul
        var statusCounts = allStatuses
            .GroupJoin(
                invoiceCounts,
                status => status.ProgressStatusId,
                count => count.ProgressStatusId,
                (status, counts) => new InvoiceStatusCountDto
                {
                    ProgressStatusId = status.ProgressStatusId,
                    ProgressStatusName = status.Name,
                    Total = counts.FirstOrDefault()?.Total ?? 0
                })
            .OrderBy(x => x.ProgressStatusId)
            .ToList();

        return new InvoiceStatusSummaryDto
        {
            StatusCounts = statusCounts
        };
    }

    /// <summary>
    /// Dashboard summary for Uang Muka by Jenis and Status (for tab Advanced, Biaya, Realisasi)
    /// </summary>
    public async Task<UangMukaStatusSummaryDto> GetUangMukaStatusSummaryAsync(string jenis)
    {
        jenis = (jenis ?? "").Trim();
        List<UangMukaStatusCountDto> statusCounts = new();

        if (jenis.Equals("Advanced", StringComparison.OrdinalIgnoreCase))
        {
            // Ambil semua status Advanced
            var allStatuses = await _context.AdvancedStatuses
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Code)
                .ToListAsync();

            // Hitung total Uang Muka per status
            var counts = await _context.UangMukas
                .Where(u => !u.IsDeleted && u.Jenis == "Advanced")
                .GroupBy(u => u.StatusId)
                .Select(g => new { StatusId = g.Key, Total = g.Count() })
                .ToListAsync();

            statusCounts = allStatuses
                .GroupJoin(
                    counts,
                    s => s.AdvancedStatusesId,
                    c => c.StatusId,
                    (status, count) => new UangMukaStatusCountDto
                    {
                        StatusId = status.AdvancedStatusesId,
                        StatusName = status.Name,
                        Total = count.FirstOrDefault()?.Total ?? 0
                    })
                .OrderBy(x => x.StatusName)
                .ToList();
        }
        else if (jenis.Equals("Biaya", StringComparison.OrdinalIgnoreCase) || jenis.Equals("Realisasi", StringComparison.OrdinalIgnoreCase))
        {
            // Ambil semua status Biaya/Realisasi
            var allStatuses = await _context.BiayaRealisasiStatuses
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Code)
                .ToListAsync();

            // Hitung total Uang Muka per status
            var counts = await _context.UangMukas
                .Where(u => !u.IsDeleted && u.Jenis == jenis)
                .GroupBy(u => u.StatusId)
                .Select(g => new { StatusId = g.Key, Total = g.Count() })
                .ToListAsync();

            statusCounts = allStatuses
                .GroupJoin(
                    counts,
                    s => s.BiayaRealisasiStatusesId,
                    c => c.StatusId,
                    (status, count) => new UangMukaStatusCountDto
                    {
                        StatusId = status.BiayaRealisasiStatusesId,
                        StatusName = status.Name,
                        Total = count.FirstOrDefault()?.Total ?? 0
                    })
                .OrderBy(x => x.StatusName)
                .ToList();
        }
        else
        {
            // Jenis tidak valid
            return new UangMukaStatusSummaryDto { StatusCounts = new List<UangMukaStatusCountDto>() };
        }

        return new UangMukaStatusSummaryDto { StatusCounts = statusCounts };
    }

    public async Task<IEnumerable<object>> GetUangMukaDetailAsync(string jenis, string status)
    {
        // Query Uang Muka data filtered by jenis and status (Status property is not mapped, so filter in-memory)
        var query = _context.UangMukas.AsQueryable();
        if (!string.IsNullOrEmpty(jenis))
            query = query.Where(u => u.Jenis == jenis);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(u => u.StatusId == status);

        var list = await query
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
        // Now filter in-memory for Status property if needed
        if (!string.IsNullOrEmpty(status))
        {
            list = list.Where(u => (status ?? u.StatusId) == status).ToList();
        }

        var result = list.Select(u => new
        {
            //u.UangMukaId,
            u.Jenis,
            u.AtasNama,
            u.Amount,
            //u.StatusId,
            Status = (jenis == "Advanced")
                        ? _context.AdvancedStatuses
                            .Where(s => s.AdvancedStatusesId == u.StatusId)
                            .Select(s => s.Name)
                            .FirstOrDefault()
                        : _context.BiayaRealisasiStatuses
                            .Where(s => s.BiayaRealisasiStatusesId == u.StatusId)
                            .Select(s => s.Name)
                            .FirstOrDefault(),
            u.NoSAP,
            u.StartDate,
            u.EndDate,
            u.Deskripsi
        });
        return result;
    }

    public async Task<IEnumerable<object>> GetContractsByStatusAsync(string status)
    {
        var today = DateTime.Now.Date;
        var next30Days = today.AddDays(30);

        var query = _context.Contracts
            .Include(c => c.Vendor)
            .Include(c => c.ContractStatus)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Active")
                // Active: kontrak aktif tapi tidak expiring soon
                query = query.Where(c => c.StartDate <= today && c.EndDate > next30Days);
            else if (status == "Expiring Soon")
                // Expiring Soon: kontrak aktif yang akan habis <= 30 hari ke depan
                query = query.Where(c => c.EndDate >= today && c.EndDate <= next30Days);
            // Tambahkan status lain jika perlu
        }

        var result = await query
            .OrderBy(c => c.EndDate)
            .Select(c => new
            {
                c.ContractNumber,
                VendorName = c.Vendor != null ? c.Vendor.VendorName : "",
                c.ContractDescription,
                c.StartDate,
                c.EndDate,
                Status = status == "Active" ? "Aman" : (status == "Expiring Soon" ? "Sudah Mau Habis" : (c.ContractStatus != null ? c.ContractStatus.Name : ""))
            })
            .ToListAsync();

        return result;
    }
}
