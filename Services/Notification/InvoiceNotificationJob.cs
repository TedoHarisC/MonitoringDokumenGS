using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;

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
        int day = today.Day;
        if (day != 1 && day != 5)
        {
            _logger.LogInformation("Invoice notification skipped: today is not 1st or 5th");
            return;
        }

        // Get all vendors with at least one invoice in this month
        var invoices = await _db.Invoices
            .Where(i => !i.IsDeleted && i.InvoiceYear == today.Year && i.InvoiceMonth == today.Month)
            .Include(i => i.Vendor)
            .ToListAsync();

        var vendorGroups = invoices
            .GroupBy(i => i.Vendor)
            .ToList();

        // Get all admin emails
        var adminEmails = await _db.Users
            .Where(u => !u.isDeleted && u.isActive)
            .Join(_db.UserRoles, u => u.UserId, ur => ur.UserId, (u, ur) => new { u, ur })
            .Join(_db.Roles, x => x.ur.RoleId, r => r.RoleId, (x, r) => new { x.u, r })
            .Where(x => x.r.Code == "ADMIN" || x.r.Code == "SUPER_ADMIN")
            .Select(x => x.u.Email)
            .Where(e => !string.IsNullOrEmpty(e))
            .Distinct()
            .ToListAsync();

        foreach (var group in vendorGroups)
        {
            var vendor = group.Key;
            var vendorEmails = _db.Users
                .Where(u => u.VendorId == vendor.VendorId && !u.isDeleted && u.isActive && !string.IsNullOrEmpty(u.Email))
                .Select(u => u.Email)
                .Distinct()
                .ToList();

            if (!vendorEmails.Any())
                continue;


            string subject, htmlBody;
            string templatePath = string.Empty;
            if (day == 1)
            {
                subject = $"[Pengingat Invoice] {vendor.VendorName} - Awal Bulan";
                templatePath = "EmailTemplates/id/InvoiceReminderDay1.html";
            }
            else // day == 5
            {
                subject = $"[Pengingat Invoice] {vendor.VendorName} - Deadline Input Invoice";
                templatePath = "EmailTemplates/id/InvoiceReminderDay5.html";
            }

            // Baca template dan replace placeholder
            htmlBody = System.IO.File.ReadAllText(templatePath)
                .Replace("{{VendorName}}", vendor.VendorName)
                .Replace("{{AppUrl}}", _appUrl);


            // Gunakan SendWithCopyAsync agar bisa multiple TO dan CC
            await _emailService.SendWithCopyAsync(
                to: vendorEmails.First(), // TO utama (wajib, ambil satu)
                subject: subject,
                htmlBody: htmlBody,
                cc: adminEmails.Concat(vendorEmails.Skip(1)).ToList() // CC: admin + sisa vendor
            );

            _logger.LogInformation("Invoice notification sent to vendor {VendorName} ({Emails}) for day {Day}", vendor.VendorName, string.Join(",", vendorEmails), day);
        }
    }
}
