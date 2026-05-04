using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using System.Globalization;
using System.Net;

public class InvoiceNotificationJob : IInvoiceNotificationJob
{
    private readonly ApplicationDBContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<InvoiceNotificationJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _appUrl;

    public InvoiceNotificationJob(
        ApplicationDBContext db,
        IEmailService emailService,
        ILogger<InvoiceNotificationJob> logger,
        IConfiguration configuration)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
        _appUrl = _configuration["AppUrl"] ?? "http://localhost:5008";
    }

    public async Task RunAsync()
    {
        var today = DateTime.Now.Date;
        var day = today.Day;
        if (day != 1 && day != 5 && day != 7 && day != 14)
        {
            _logger.LogInformation("Invoice notification skipped: today is not 1st, 5th, 7th, or 14th");
            return;
        }

        if (day == 1 || day == 5)
        {
            await ExecuteVendorReminderAsync(templateDay: day, today: today, mode: "scheduled");
            return;
        }

        await ExecuteAdminSummaryAsync(templateDay: day, today: today, mode: "scheduled");
    }

    public async Task RunTrialAsync(int templateDay, List<string>? overrideToEmails = null)
    {
        if (templateDay != 1 && templateDay != 5 && templateDay != 7 && templateDay != 14)
        {
            throw new ArgumentException("Template day must be 1, 5, 7, or 14", nameof(templateDay));
        }

        if (templateDay == 1 || templateDay == 5)
        {
            await ExecuteVendorReminderAsync(
                templateDay: templateDay,
                today: DateTime.Now.Date,
                mode: "trial",
                overrideToEmails: overrideToEmails);
            return;
        }

        await ExecuteAdminSummaryAsync(templateDay: templateDay, today: DateTime.Now.Date, mode: "trial");
    }

    private async Task ExecuteVendorReminderAsync(int templateDay, DateTime today, string mode, List<string>? overrideToEmails = null)
    {
        var day = templateDay;

        var previousMonthDate = today.AddMonths(-1);
        var targetYear = previousMonthDate.Year;
        var targetMonth = previousMonthDate.Month;

        _logger.LogInformation(
            "Invoice reminder started in {Mode} mode with template day {Day}. Target invoice period: {Year}/{Month}",
            mode,
            day,
            targetYear,
            targetMonth);

        var submittedVendorIds = await _db.Invoices
            .Where(i => !i.IsDeleted && i.InvoiceYear == targetYear && i.InvoiceMonth == targetMonth)
            .Select(i => i.VendorId)
            .Distinct()
            .ToListAsync();

        var vendorsToRemind = await _db.Vendors
            .Where(v => !v.IsDeleted && !submittedVendorIds.Contains(v.VendorId))
            .Select(v => new { v.VendorId, v.VendorName })
            .ToListAsync();

        _logger.LogInformation(
            "Invoice reminder target vendors: {TargetCount}. Vendors already submitted: {SubmittedCount}",
            vendorsToRemind.Count,
            submittedVendorIds.Count);

        var hasOverrideRecipients = overrideToEmails != null && overrideToEmails.Any();

        if (!vendorsToRemind.Any() && !hasOverrideRecipients)
        {
            _logger.LogInformation(
                "Invoice reminder finished: no vendors pending for period {Year}/{Month}",
                targetYear,
                targetMonth);
            return;
        }

        var adminEmails = await GetAdminEmailsAsync();

        string subject;
        string templateFileName;
        if (day == 1)
        {
            subject = "[Pengingat Invoice] Reminder Awal Bulan - Invoice Belum Dikirim";
            templateFileName = "InvoiceReminderDay1.html";
        }
        else // day == 5
        {
            subject = "[Pengingat Invoice] Reminder Deadline - Invoice Belum Dikirim";
            templateFileName = "InvoiceReminderDay5.html";
        }

        var relativeTemplatePath = Path.Combine("EmailTemplates", "id", templateFileName);
        var templatePath = ResolveTemplatePath(relativeTemplatePath);
        if (templatePath is null)
        {
            _logger.LogError(
                "Invoice reminder template not found. Checked: {BasePath} and {CurrentPath}",
                Path.Combine(AppContext.BaseDirectory, relativeTemplatePath),
                Path.Combine(Directory.GetCurrentDirectory(), relativeTemplatePath));
            return;
        }

        var templateHtml = await File.ReadAllTextAsync(templatePath);
        List<string> vendorEmails;
        if (hasOverrideRecipients)
        {
            vendorEmails = overrideToEmails!
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToList();

            _logger.LogInformation(
                "Invoice reminder trial uses override recipients. Recipient count: {RecipientCount}",
                vendorEmails.Count);
        }
        else
        {
            var vendorIdsToRemind = vendorsToRemind.Select(v => v.VendorId).ToList();
            vendorEmails = await _db.Users
                .Where(u => vendorIdsToRemind.Contains(u.VendorId) && !u.isDeleted && u.isActive && !string.IsNullOrWhiteSpace(u.Email))
                .Select(u => u.Email)
                .Distinct()
                .ToListAsync();
        }

        if (!vendorEmails.Any())
        {
            _logger.LogWarning(
                "Invoice reminder skipped: no active vendor emails found for {VendorCount} vendors in period {Year}/{Month}",
                vendorsToRemind.Count,
                targetYear,
                targetMonth);
            return;
        }

        var periodLabel = previousMonthDate.ToString("MMMM yyyy");
        var htmlBody = templateHtml
            .Replace("{{TargetPeriod}}", periodLabel)
            .Replace("{{AppUrl}}", _appUrl);

        try
        {
            await _emailService.SendWithCopyMultipleToAsync(
                toAddresses: vendorEmails,
                subject: subject,
                htmlBody: htmlBody,
                cc: adminEmails);

            _logger.LogInformation(
                "Invoice reminder completed in {Mode} mode. Sent 1 email to {VendorEmailCount} vendor recipients with {AdminEmailCount} admin CC for period {Year}/{Month}",
                mode,
                vendorEmails.Count,
                adminEmails.Count,
                targetYear,
                targetMonth);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send global invoice reminder in {Mode} mode for period {Year}/{Month}",
                mode,
                targetYear,
                targetMonth);
            throw;
        }
    }

    private async Task ExecuteAdminSummaryAsync(int templateDay, DateTime today, string mode)
    {
        var previousMonthDate = today.AddMonths(-1);
        var targetYear = previousMonthDate.Year;
        var targetMonth = previousMonthDate.Month;
        var periodLabel = previousMonthDate.ToString("MMMM yyyy", new CultureInfo("id-ID"));
        var reportDate = today.ToString("dd MMMM yyyy", new CultureInfo("id-ID"));

        _logger.LogInformation(
            "Invoice admin summary started in {Mode} mode on day {Day}. Target period: {Year}/{Month}",
            mode,
            templateDay,
            targetYear,
            targetMonth);

        var adminEmails = await GetAdminEmailsAsync();
        if (!adminEmails.Any())
        {
            _logger.LogWarning("Invoice admin summary skipped: no active admin emails found");
            return;
        }

        var submittedVendorIds = await _db.Invoices
            .Where(i => !i.IsDeleted && i.InvoiceYear == targetYear && i.InvoiceMonth == targetMonth)
            .Select(i => i.VendorId)
            .Distinct()
            .ToListAsync();

        var activeVendorsForSummary = await _db.Vendors
            .Where(v => !v.IsDeleted)
            .Select(v => new { v.VendorId, v.VendorCode, v.VendorName })
            .ToListAsync();

        var excludedVendorCode = "ABB";
        var excludedVendorName = "Asmin Bara Bronang";

        var filteredVendors = activeVendorsForSummary
            .Where(v => !string.Equals(v.VendorCode?.Trim(), excludedVendorCode, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(v.VendorName?.Trim(), excludedVendorName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var pendingVendors = filteredVendors
            .Where(v => !submittedVendorIds.Contains(v.VendorId))
            .OrderBy(v => v.VendorName)
            .Select(v => new { v.VendorCode, v.VendorName })
            .ToList();

        var totalActiveVendors = filteredVendors.Count;
        var submittedCount = totalActiveVendors - pendingVendors.Count;

        string subject;
        string relativeTemplatePath;
        string htmlBody;

        if (pendingVendors.Any())
        {
            subject = templateDay == 7
                ? $"[Invoice] Laporan Vendor Belum Submit - Monitoring Tanggal 7 ({periodLabel})"
                : $"[Invoice] Laporan Vendor Belum Submit - Monitoring Tanggal 14 ({periodLabel})";
            relativeTemplatePath = Path.Combine("EmailTemplates", "id", "InvoiceAdminPendingSummary.html");

            var templatePath = ResolveTemplatePath(relativeTemplatePath);
            if (templatePath is null)
            {
                _logger.LogError(
                    "Invoice admin pending summary template not found. Checked: {BasePath} and {CurrentPath}",
                    Path.Combine(AppContext.BaseDirectory, relativeTemplatePath),
                    Path.Combine(Directory.GetCurrentDirectory(), relativeTemplatePath));
                return;
            }

            var rowHtml = string.Join("", pendingVendors.Select((vendor, index) =>
                $"<tr><td style=\"padding:12px 10px; border-bottom:1px solid #e5e7eb; color:#0f172a;\">{index + 1}</td><td style=\"padding:12px 10px; border-bottom:1px solid #e5e7eb; color:#0f172a;\">{WebUtility.HtmlEncode(vendor.VendorCode)}</td><td style=\"padding:12px 10px; border-bottom:1px solid #e5e7eb; color:#0f172a;\">{WebUtility.HtmlEncode(vendor.VendorName)}</td></tr>"));

            htmlBody = (await File.ReadAllTextAsync(templatePath))
                .Replace("{{TargetPeriod}}", periodLabel)
                .Replace("{{ReportDate}}", reportDate)
                .Replace("{{PendingCount}}", pendingVendors.Count.ToString())
                .Replace("{{SubmittedCount}}", submittedCount.ToString())
                .Replace("{{TotalVendorCount}}", totalActiveVendors.ToString())
                .Replace("{{VendorRows}}", rowHtml)
                .Replace("{{AppUrl}}", _appUrl);
        }
        else
        {
            subject = $"[Invoice] Semua Vendor Sudah Submit ({periodLabel})";
            relativeTemplatePath = Path.Combine("EmailTemplates", "id", "InvoiceAdminAllSubmitted.html");

            var templatePath = ResolveTemplatePath(relativeTemplatePath);
            if (templatePath is null)
            {
                _logger.LogError(
                    "Invoice admin all-submitted template not found. Checked: {BasePath} and {CurrentPath}",
                    Path.Combine(AppContext.BaseDirectory, relativeTemplatePath),
                    Path.Combine(Directory.GetCurrentDirectory(), relativeTemplatePath));
                return;
            }

            htmlBody = (await File.ReadAllTextAsync(templatePath))
                .Replace("{{TargetPeriod}}", periodLabel)
                .Replace("{{ReportDate}}", reportDate)
                .Replace("{{TotalVendorCount}}", totalActiveVendors.ToString())
                .Replace("{{AppUrl}}", _appUrl);
        }

        try
        {
            await _emailService.SendToMultipleAsync(adminEmails, subject, htmlBody);
            _logger.LogInformation(
                "Invoice admin summary completed in {Mode} mode. Sent to {AdminCount} admin recipients. Pending vendors: {PendingCount}",
                mode,
                adminEmails.Count,
                pendingVendors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send invoice admin summary in {Mode} mode for period {Year}/{Month}",
                mode,
                targetYear,
                targetMonth);
            throw;
        }
    }

    private async Task<List<string>> GetAdminEmailsAsync()
    {
        return await _db.Users
            .Where(u => !u.isDeleted && u.isActive)
            .Join(_db.UserRoles, u => u.UserId, ur => ur.UserId, (u, ur) => new { u, ur })
            .Join(_db.Roles, x => x.ur.RoleId, r => r.RoleId, (x, r) => new { x.u, r })
            .Where(x => x.r.Code == "ADMIN" || x.r.Code == "SUPER_ADMIN")
            .Select(x => x.u.Email)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct()
            .ToListAsync();
    }

    private static string? ResolveTemplatePath(string relativeTemplatePath)
    {
        var fromBaseDirectory = Path.Combine(AppContext.BaseDirectory, relativeTemplatePath);
        if (File.Exists(fromBaseDirectory))
        {
            return fromBaseDirectory;
        }

        var fromCurrentDirectory = Path.Combine(Directory.GetCurrentDirectory(), relativeTemplatePath);
        if (File.Exists(fromCurrentDirectory))
        {
            return fromCurrentDirectory;
        }

        return null;
    }
}
