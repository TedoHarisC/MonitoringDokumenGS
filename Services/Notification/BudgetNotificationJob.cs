using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Services.Infrastructure;
using MonitoringDokumenGS.Dtos.Infrastructure;

namespace MonitoringDokumenGS.Services.Notification
{
    public class BudgetNotificationJob : IBudgetNotificationJob
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;
        private readonly IAuditLog _auditLog;
        private readonly ILogger<BudgetNotificationJob> _logger;
        private readonly INotifications _notificationService;

        public BudgetNotificationJob(
            ApplicationDBContext context,
            IEmailService emailService,
            IAuditLog auditLog,
            ILogger<BudgetNotificationJob> logger,
            INotifications notificationService)
        {
            _context = context;
            _emailService = emailService;
            _auditLog = auditLog;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task RunAsync()
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                _logger.LogInformation("Starting budget overbudget check for {Year}/{Month}", currentYear, currentMonth);

                // Get budget status
                var budgetStatus = await GetBudgetStatusAsync(currentYear);

                if (budgetStatus == null)
                {
                    _logger.LogWarning("No budget configured for year {Year}", currentYear);
                    return;
                }

                // Check overall budget status
                if (budgetStatus.IsOverBudget)
                {
                    await SendOverbudgetAlert(
                        "YEARLY",
                        currentYear,
                        null,
                        budgetStatus.TotalBudget,
                        budgetStatus.TotalSpent,
                        budgetStatus.BudgetUtilizationPercent
                    );
                }

                // Check monthly budget status
                foreach (var monthStatus in budgetStatus.MonthlyStatus)
                {
                    if (monthStatus.IsOverBudget)
                    {
                        await SendOverbudgetAlert(
                            "MONTHLY",
                            currentYear,
                            monthStatus.Month,
                            monthStatus.Budget,
                            monthStatus.Spent,
                            monthStatus.UtilizationPercent
                        );
                    }
                    else if (monthStatus.IsNearLimit && monthStatus.Month == currentMonth)
                    {
                        // Send warning for current month if near limit (>90%)
                        await SendBudgetWarningAlert(
                            currentYear,
                            monthStatus.Month,
                            monthStatus.Budget,
                            monthStatus.Spent,
                            monthStatus.UtilizationPercent
                        );
                    }
                }

                _logger.LogInformation("Budget overbudget check completed for {Year}", currentYear);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running budget overbudget check");
                throw;
            }
        }

        public async Task<BudgetOverviewResult> GetBudgetStatusAsync(int year)
        {
            var budget = await _context.MST_Budget
                .Where(b => b.Year == year)
                .FirstOrDefaultAsync();

            if (budget == null)
                return null!;

            // Get total spending for the year
            var totalSpent = await _context.Invoices
                .Where(i => i.InvoiceYear == year && !i.IsDeleted)
                .SumAsync(i => i.InvoiceAmount);

            // Get monthly spending
            var monthlySpending = await _context.Invoices
                .Where(i => i.InvoiceYear == year && !i.IsDeleted)
                .GroupBy(i => i.InvoiceMonth)
                .Select(g => new
                {
                    Month = g.Key,
                    Spent = g.Sum(i => i.InvoiceAmount)
                })
                .ToListAsync();

            var monthNames = new[] { "", "January", "February", "March", "April", "May", "June",
                                    "July", "August", "September", "October", "November", "December" };

            var monthlyStatus = new List<MonthlyBudgetStatus>();
            for (int month = 1; month <= 12; month++)
            {
                var spent = monthlySpending.FirstOrDefault(m => m.Month == month)?.Spent ?? 0;
                var utilizationPercent = budget.MonthlyBudget > 0 ? (spent / budget.MonthlyBudget) * 100 : 0;

                monthlyStatus.Add(new MonthlyBudgetStatus
                {
                    Month = month,
                    MonthName = monthNames[month],
                    Budget = budget.MonthlyBudget,
                    Spent = spent,
                    Remaining = budget.MonthlyBudget - spent,
                    UtilizationPercent = utilizationPercent,
                    IsOverBudget = spent > budget.MonthlyBudget,
                    IsNearLimit = utilizationPercent >= 90 && utilizationPercent <= 100
                });
            }

            var remainingBudget = budget.TotalBudget - totalSpent;
            var budgetUtilization = budget.TotalBudget > 0 ? (totalSpent / budget.TotalBudget) * 100 : 0;

            return new BudgetOverviewResult
            {
                Year = year,
                TotalBudget = budget.TotalBudget,
                MonthlyBudget = budget.MonthlyBudget,
                TotalSpent = totalSpent,
                RemainingBudget = remainingBudget,
                BudgetUtilizationPercent = budgetUtilization,
                IsOverBudget = totalSpent > budget.TotalBudget,
                MonthlyStatus = monthlyStatus
            };
        }

        private async Task SendOverbudgetAlert(
            string type, // "YEARLY" or "MONTHLY"
            int year,
            int? month,
            decimal budget,
            decimal spent,
            decimal utilizationPercent)
        {
            try
            {
                var monthName = month.HasValue
                    ? new[] { "", "January", "February", "March", "April", "May", "June",
                             "July", "August", "September", "October", "November", "December" }[month.Value]
                    : null;

                var periodText = type == "YEARLY"
                    ? $"Year {year}"
                    : $"{monthName} {year}";

                var excess = spent - budget;
                var excessPercent = utilizationPercent - 100;

                var title = $"⚠️ BUDGET OVERRUN ALERT - {periodText}";
                var message = $@"
                    <strong>Budget Exceeded!</strong><br/>
                    Period: {periodText}<br/>
                    Budget: {budget:N2}<br/>
                    Spent: {spent:N2}<br/>
                    Over Budget: {excess:N2} ({excessPercent:N1}%)<br/>
                    Utilization: {utilizationPercent:N1}%<br/><br/>
                    Immediate action required!
                ";

                // Get admin users to notify
                var adminUsers = await _context.Users
                    .Where(u => !u.isDeleted && u.isActive)
                    .Join(_context.UserRoles,
                        u => u.UserId,
                        ur => ur.UserId,
                        (u, ur) => new { u, ur })
                    .Join(_context.Roles,
                        x => x.ur.RoleId,
                        r => r.RoleId,
                        (x, r) => new { x.u, r })
                    .Where(x => x.r.Code == "SUPER_ADMIN" || x.r.Code == "ADMIN")
                    .Select(x => new { x.u.UserId, x.u.Email, x.u.Username })
                    .Distinct()
                    .ToListAsync();

                foreach (var admin in adminUsers)
                {
                    // Create in-app notification
                    await _notificationService.CreateAsync(new NotificationDto
                    {
                        UserId = admin.UserId,
                        Title = title,
                        Message = message.Replace("<strong>", "").Replace("</strong>", "").Replace("<br/>", "\n"),
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });

                    // Send email notification
                    if (!string.IsNullOrEmpty(admin.Email))
                    {
                        var emailBody = EmailTemplateHelper.GetNotificationEmail(
                            title: title,
                            message: $"Budget has been exceeded for {periodText}.",
                            referenceId: $"BUDGET-{type}-{year}" + (month.HasValue ? $"-{month:00}" : ""),
                            type: "Budget Alert",
                            date: DateTime.Now.ToString("MMMM dd, yyyy HH:mm"),
                            actionLink: $"http://localhost:5008/Master/Budget",
                            actionButtonText: "View Budget Details",
                            iconBackgroundColor: "#dc3545",
                            icon: "⚠️"
                        );

                        await _emailService.SendAsync(
                            to: admin.Email,
                            subject: title,
                            htmlBody: emailBody
                        );

                        _logger.LogInformation("Overbudget alert sent to {Email} for {Period}", admin.Email, periodText);
                    }
                }

                // Log to audit
                await _auditLog.LogAsync(
                    entityName: "Budget",
                    action: "BUDGET_OVERRUN_ALERT",
                    oldData: null,
                    newData: new { Type = type, Year = year, Month = month, Excess = excess, ExcessPercent = excessPercent },
                    entityKey: $"{type}_{year}" + (month.HasValue ? $"_{month:00}" : "")
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send overbudget alert for {Type} {Year}/{Month}", type, year, month);
            }
        }

        private async Task SendBudgetWarningAlert(
            int year,
            int month,
            decimal budget,
            decimal spent,
            decimal utilizationPercent)
        {
            try
            {
                var monthName = new[] { "", "January", "February", "March", "April", "May", "June",
                                       "July", "August", "September", "October", "November", "December" }[month];

                var remaining = budget - spent;

                var title = $"⚡ Budget Warning - {monthName} {year}";
                var message = $@"
                    <strong>Budget Approaching Limit</strong><br/>
                    Period: {monthName} {year}<br/>
                    Budget: {budget:N2}<br/>
                    Spent: {spent:N2}<br/>
                    Remaining: {remaining:N2}<br/>
                    Utilization: {utilizationPercent:N1}%<br/><br/>
                    Please monitor spending carefully.
                ";

                // Get admin users
                var adminUsers = await _context.Users
                    .Where(u => !u.isDeleted && u.isActive)
                    .Join(_context.UserRoles,
                        u => u.UserId,
                        ur => ur.UserId,
                        (u, ur) => new { u, ur })
                    .Join(_context.Roles,
                        x => x.ur.RoleId,
                        r => r.RoleId,
                        (x, r) => new { x.u, r })
                    .Where(x => x.r.Code == "SUPER_ADMIN" || x.r.Code == "ADMIN")
                    .Select(x => new { x.u.UserId, x.u.Email })
                    .Distinct()
                    .ToListAsync();

                foreach (var admin in adminUsers)
                {
                    // Create in-app notification
                    await _notificationService.CreateAsync(new NotificationDto
                    {
                        UserId = admin.UserId,
                        Title = title,
                        Message = message.Replace("<strong>", "").Replace("</strong>", "").Replace("<br/>", "\n"),
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });

                    // Send email (optional - less urgent than overbudget)
                    if (!string.IsNullOrEmpty(admin.Email))
                    {
                        var emailBody = EmailTemplateHelper.GetNotificationEmail(
                            title: title,
                            message: $"Budget utilization has reached {utilizationPercent:N1}% for {monthName} {year}.",
                            referenceId: $"BUDGET-WARNING-{year}-{month:00}",
                            type: "Budget Warning",
                            date: DateTime.Now.ToString("MMMM dd, yyyy HH:mm"),
                            actionLink: $"http://localhost:5008/Master/Budget",
                            actionButtonText: "View Budget",
                            iconBackgroundColor: "#ffc107",
                            icon: "⚡"
                        );

                        await _emailService.SendAsync(
                            to: admin.Email,
                            subject: title,
                            htmlBody: emailBody
                        );
                    }
                }

                _logger.LogInformation("Budget warning sent for {Month}/{Year} - {Utilization}%", month, year, utilizationPercent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send budget warning for {Year}/{Month}", year, month);
            }
        }
    }
}
