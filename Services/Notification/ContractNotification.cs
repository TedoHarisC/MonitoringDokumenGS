using Dapper;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Dtos;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Models.Infrastructure;
using MonitoringDokumenGS.Services.Infrastructure;
using MonitoringDokumenGS.Dtos.Infrastructure;

public class ContractNotificationJob : IContractNotificationJob
{
    private readonly ApplicationDBContext _db;
    private readonly IEmailService _email;
    private readonly IAuditLog _audit;
    private readonly INotificationLog _notificationLog;
    private readonly INotifications _notificationService;
    private readonly ILogger<ContractNotificationJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _appUrl;

    public ContractNotificationJob(
        ApplicationDBContext db,
        IEmailService email,
        IAuditLog audit,
        INotificationLog notificationLog,
        INotifications notificationService,
        ILogger<ContractNotificationJob> logger,
        IConfiguration configuration)
    {
        _db = db;
        _email = email;
        _audit = audit;
        _notificationLog = notificationLog;
        _notificationService = notificationService;
        _logger = logger;
        _configuration = configuration;
        _appUrl = _configuration["AppUrl"] ?? "http://localhost:5008";
    }

    public async Task RunAsync()
    {
        try
        {
            _logger.LogInformation("Starting contract expiring notification job");

            var today = DateTime.Now.Date;

            // Get contracts that are expiring
            var expiringContracts = await _db.Contracts
                .Where(c => !c.IsDeleted && c.EndDate >= today)
                .Select(c => new
                {
                    c.ContractId,
                    c.ContractNumber,
                    c.VendorId,
                    c.EndDate,
                    c.CreatedByUserId
                })
                .ToListAsync();

            _logger.LogInformation("Found {Count} active contracts to check", expiringContracts.Count);

            foreach (var contract in expiringContracts)
            {
                var daysLeft = (int)(contract.EndDate - today).TotalDays;

                if (daysLeft < 0)
                    continue;

                // Determine reminder level and notification type
                string notificationType;
                int reminderLevel;
                string urgencyLevel;

                if (daysLeft <= 3)
                {
                    notificationType = NotificationTypes.CONTRACT_EXPIRING_FINAL;
                    reminderLevel = 5;
                    urgencyLevel = "CRITICAL";
                }
                else if (daysLeft <= 7)
                {
                    notificationType = NotificationTypes.CONTRACT_EXPIRING_7DAYS;
                    reminderLevel = 4;
                    urgencyLevel = "URGENT";
                }
                else if (daysLeft <= 14)
                {
                    notificationType = NotificationTypes.CONTRACT_EXPIRING_14DAYS;
                    reminderLevel = 3;
                    urgencyLevel = "HIGH";
                }
                else if (daysLeft <= 30)
                {
                    notificationType = NotificationTypes.CONTRACT_EXPIRING_30DAYS;
                    reminderLevel = 2;
                    urgencyLevel = "MEDIUM";
                }
                else if (daysLeft <= 60)
                {
                    notificationType = NotificationTypes.CONTRACT_EXPIRING_60DAYS;
                    reminderLevel = 1;
                    urgencyLevel = "LOW";
                }
                else
                {
                    // Contract expires more than 60 days away, skip
                    continue;
                }

                // Anti-spam check: Check if we already sent this reminder level
                var referenceId = contract.ContractId.ToString();
                var lastReminderLevel = await _notificationLog.GetContractReminderLevelAsync(referenceId);

                // Only send if this is a new reminder level (higher than last sent)
                if (reminderLevel <= lastReminderLevel)
                {
                    _logger.LogDebug(
                        "Skipping contract {ContractNumber} - reminder level {Level} already sent",
                        contract.ContractNumber, reminderLevel);
                    continue;
                }

                // For final reminders (≤3 days), send daily
                int cooldownHours = daysLeft <= 3 ? 24 : 72; // Daily for critical, else 3 days

                var canSend = await _notificationLog.CanSendNotificationAsync(
                    notificationType,
                    referenceId,
                    cooldownHours);

                if (!canSend)
                {
                    _logger.LogDebug(
                        "Skipping contract {ContractNumber} - cooldown active",
                        contract.ContractNumber);
                    continue;
                }

                await SendContractExpiringNotificationAsync(
                    contract.ContractId,
                    contract.ContractNumber,
                    contract.VendorId,
                    contract.EndDate,
                    daysLeft,
                    contract.CreatedByUserId,
                    notificationType,
                    reminderLevel,
                    urgencyLevel
                );
            }

            _logger.LogInformation("Contract expiring notification job completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running contract expiring notification job");
            throw;
        }
    }

    private async Task SendContractExpiringNotificationAsync(
        Guid contractId,
        string contractNumber,
        Guid vendorId,
        DateTime endDate,
        int daysLeft,
        Guid? createdByUserId,
        string notificationType,
        int reminderLevel,
        string urgencyLevel)
    {
        try
        {
            // Get vendor info
            var vendor = await _db.Vendors
                .Where(v => v.VendorId == vendorId)
                .FirstOrDefaultAsync();

            var vendorName = vendor?.VendorName ?? "Unknown Vendor";

            // Get contract PIC (creator)
            var picUser = createdByUserId.HasValue
                ? await _db.Users.FindAsync(createdByUserId.Value)
                : null;

            // Build notification message
            var icon = urgencyLevel switch
            {
                "CRITICAL" => "🚨",
                "URGENT" => "⚠️",
                "HIGH" => "⏰",
                "MEDIUM" => "📅",
                _ => "📋"
            };

            var title = $"{icon} {urgencyLevel}: Contract Expiring in {daysLeft} {(daysLeft == 1 ? "Day" : "Days")}";
            var message = $@"
                <strong>Contract: {contractNumber}</strong><br/>
                Vendor: {vendorName}<br/>
                End Date: {endDate:MMMM dd, yyyy}<br/>
                Days Remaining: {daysLeft}<br/>
                Priority: {urgencyLevel}<br/><br/>
                {GetActionMessage(daysLeft)}
            ";

            var emailSubject = $"{urgencyLevel}: Contract {contractNumber} Expires in {daysLeft} Days";

            // Send to contract PIC if exists
            if (picUser != null && !string.IsNullOrEmpty(picUser.Email))
            {
                // Create in-app notification
                await _notificationService.CreateAsync(new NotificationDto
                {
                    UserId = picUser.UserId,
                    Title = title,
                    Message = message.Replace("<strong>", "").Replace("</strong>", "").Replace("<br/>", "\n"),
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });

                // Send email
                var emailBody = EmailTemplateHelper.GetContractExpiringEmail(
                    contractNumber: contractNumber,
                    vendorName: vendorName,
                    endDate: endDate.ToString("MMMM dd, yyyy"),
                    daysLeft: daysLeft.ToString(),
                    actionLink: $"{_appUrl}/Contract/Detail/{contractId}",
                    language: "en"
                );

                await _email.SendAsync(
                    to: picUser.Email,
                    subject: emailSubject,
                    htmlBody: emailBody
                );

                _logger.LogInformation(
                    "Contract expiring notification sent to PIC {Email} for contract {ContractNumber}",
                    picUser.Email, contractNumber);

                // Log notification
                await _notificationLog.LogNotificationSentAsync(
                    notificationType,
                    contractId.ToString(),
                    picUser.Email,
                    reminderLevel,
                    $"Days left: {daysLeft}, Level: {reminderLevel}"
                );
            }

            // Also notify admins for urgent/critical contracts
            if (daysLeft <= 14)
            {
                var adminUsers = await _db.Users
                    .Where(u => !u.isDeleted && u.isActive)
                    .Join(_db.UserRoles, u => u.UserId, ur => ur.UserId, (u, ur) => new { u, ur })
                    .Join(_db.Roles, x => x.ur.RoleId, r => r.RoleId, (x, r) => new { x.u, r })
                    .Where(x => x.r.Code == "ADMIN" || x.r.Code == "SUPER_ADMIN")
                    .Select(x => new { x.u.UserId, x.u.Email })
                    .Distinct()
                    .ToListAsync();

                foreach (var admin in adminUsers)
                {
                    // Skip if this admin is already the PIC
                    if (admin.UserId == createdByUserId)
                        continue;

                    await _notificationService.CreateAsync(new NotificationDto
                    {
                        UserId = admin.UserId,
                        Title = title,
                        Message = message.Replace("<strong>", "").Replace("</strong>", "").Replace("<br/>", "\n"),
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });

                    if (!string.IsNullOrEmpty(admin.Email))
                    {
                        var emailBody = EmailTemplateHelper.GetContractExpiringEmail(
                            contractNumber: contractNumber,
                            vendorName: vendorName,
                            endDate: endDate.ToString("MMMM dd, yyyy"),
                            daysLeft: daysLeft.ToString(),
                            actionLink: $"{_appUrl}/Contract/Detail/{contractId}",
                            language: "en"
                        );

                        await _email.SendAsync(
                            to: admin.Email,
                            subject: emailSubject,
                            htmlBody: emailBody
                        );

                        // Log notification
                        await _notificationLog.LogNotificationSentAsync(
                            notificationType,
                            contractId.ToString(),
                            admin.Email,
                            reminderLevel,
                            $"Days left: {daysLeft}, Admin notification"
                        );
                    }
                }

                _logger.LogInformation(
                    "Contract expiring notification sent to {Count} admins for contract {ContractNumber}",
                    adminUsers.Count, contractNumber);
            }

            // Log to audit
            await _audit.LogAsync(
                entityName: "Contract",
                action: "CONTRACT_EXPIRY_REMINDER",
                oldData: null,
                newData: new { ContractNumber = contractNumber, DaysLeft = daysLeft, ReminderLevel = reminderLevel },
                entityKey: contractId.ToString()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contract expiring notification for contract {ContractNumber}", contractNumber);
        }
    }

    private string GetActionMessage(int daysLeft)
    {
        return daysLeft switch
        {
            <= 3 => "⚠️ <strong>IMMEDIATE ACTION REQUIRED!</strong> Please renew or extend this contract immediately.",
            <= 7 => "🔥 <strong>URGENT!</strong> Contract expires this week. Start renewal process now.",
            <= 14 => "⏰ <strong>ACTION REQUIRED</strong> - Please initiate renewal process.",
            <= 30 => "📅 Contract renewal should be planned soon.",
            <= 60 => "📋 Early notice: Please review and plan for renewal.",
            _ => "Contract expiring soon."
        };
    }
}
