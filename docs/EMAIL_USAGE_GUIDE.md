# Cara Menggunakan Email Templates - Panduan Praktis

## 📧 Setup & Configuration

### 1. Configure Email di appsettings.json

```json
{
  "Email": {
    "Provider": "SMTP",
    "FromName": "Monitoring GS",
    "FromEmail": "noreply@company.com",
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "your-email@gmail.com",
      "Password": "your-app-password",
      "UseSsl": true
    }
  }
}
```

### 2. Email Providers Configuration

#### **Gmail SMTP**

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "UseSsl": true
  }
}
```

**Note:** Gunakan App Password, bukan password biasa

- Go to: https://myaccount.google.com/apppasswords
- Generate app password untuk "Mail"

#### **Outlook/Hotmail SMTP**

```json
{
  "Smtp": {
    "Host": "smtp-mail.outlook.com",
    "Port": 587,
    "Username": "your-email@outlook.com",
    "Password": "your-password",
    "UseSsl": true
  }
}
```

#### **Office 365 SMTP**

```json
{
  "Smtp": {
    "Host": "smtp.office365.com",
    "Port": 587,
    "Username": "your-email@yourdomain.com",
    "Password": "your-password",
    "UseSsl": true
  }
}
```

#### **Custom SMTP Server**

```json
{
  "Smtp": {
    "Host": "mail.yourdomain.com",
    "Port": 587,
    "Username": "noreply@yourdomain.com",
    "Password": "your-password",
    "UseSsl": true
  }
}
```

---

## 🚀 Cara Menggunakan

### Metode 1: Inject IEmailService di Service/Controller

```csharp
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Services.Infrastructure;

public class InvoiceService : IInvoice
{
    private readonly IEmailService _emailService;
    private readonly INotifications _notificationService;

    public InvoiceService(
        ApplicationDBContext context,
        IEmailService emailService,
        INotifications notificationService)
    {
        _context = context;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<InvoiceDto> CreateAsync(InvoiceDto dto)
    {
        // ... create invoice logic ...

        // Send notification
        await _notificationService.CreateAsync(new NotificationDto
        {
            UserId = dto.CreatedByUserId,
            Title = "Invoice Created",
            Message = $"Invoice {invoice.InvoiceNumber} has been created"
        });

        // Send email notification
        var user = await _context.Users.FindAsync(dto.CreatedByUserId);
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            var htmlBody = EmailTemplateHelper.GetInvoiceCreatedEmail(
                invoiceNumber: invoice.InvoiceNumber,
                vendorName: vendor.VendorName,
                amount: $"Rp {invoice.Amount:N0}",
                dueDate: invoice.DueDate?.ToString("MMMM dd, yyyy") ?? "N/A",
                status: "Pending",
                createdBy: user.Username,
                createdDate: DateTime.Now.ToString("MMMM dd, yyyy"),
                description: invoice.Description ?? "",
                invoiceLink: $"https://yourapp.com/invoice/{invoice.InvoiceId}"
            );

            try
            {
                await _emailService.SendAsync(
                    to: user.Email,
                    subject: $"New Invoice Created - {invoice.InvoiceNumber}",
                    htmlBody: htmlBody
                );
            }
            catch (Exception ex)
            {
                // Log error but don't fail the operation
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        return invoice.ToDto();
    }
}
```

---

### Metode 2: Create Helper Method di NotificationHelper

Update file `NotificationHelper.cs`:

```csharp
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Dtos.Infrastructure;
using MonitoringDokumenGS.Services.Infrastructure;

namespace MonitoringDokumenGS.Services.Infrastructure
{
    public static class NotificationHelper
    {
        // ... existing methods ...

        /// <summary>
        /// Notify invoice created dengan email
        /// </summary>
        public static async Task NotifyInvoiceCreatedWithEmailAsync(
            INotifications notificationService,
            IEmailService emailService,
            Guid userId,
            string userEmail,
            string userName,
            string invoiceNumber,
            string vendorName,
            decimal amount,
            DateTime? dueDate,
            string description,
            string invoiceLink)
        {
            // Create in-app notification
            await NotifyInvoiceCreatedAsync(notificationService, userId, invoiceNumber);

            // Send email if email is provided
            if (!string.IsNullOrEmpty(userEmail))
            {
                try
                {
                    var htmlBody = EmailTemplateHelper.GetInvoiceCreatedEmail(
                        invoiceNumber: invoiceNumber,
                        vendorName: vendorName,
                        amount: $"Rp {amount:N0}",
                        dueDate: dueDate?.ToString("MMMM dd, yyyy") ?? "N/A",
                        status: "Pending",
                        createdBy: userName,
                        createdDate: DateTime.Now.ToString("MMMM dd, yyyy"),
                        description: description,
                        invoiceLink: invoiceLink
                    );

                    await emailService.SendAsync(
                        to: userEmail,
                        subject: $"New Invoice Created - {invoiceNumber}",
                        htmlBody: htmlBody
                    );
                }
                catch (Exception ex)
                {
                    // Log error but don't fail
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Notify contract created dengan email
        /// </summary>
        public static async Task NotifyContractCreatedWithEmailAsync(
            INotifications notificationService,
            IEmailService emailService,
            Guid userId,
            string userEmail,
            string userName,
            string contractNumber,
            string vendorName,
            decimal contractValue,
            DateTime? startDate,
            DateTime? endDate,
            string description,
            string contractLink)
        {
            // Create in-app notification
            await NotifyContractCreatedAsync(notificationService, userId, contractNumber);

            // Send email
            if (!string.IsNullOrEmpty(userEmail))
            {
                try
                {
                    var htmlBody = EmailTemplateHelper.GetContractCreatedEmail(
                        contractNumber: contractNumber,
                        vendorName: vendorName,
                        contractValue: $"Rp {contractValue:N0}",
                        startDate: startDate?.ToString("MMMM dd, yyyy") ?? "N/A",
                        endDate: endDate?.ToString("MMMM dd, yyyy") ?? "N/A",
                        status: "Active",
                        createdBy: userName,
                        createdDate: DateTime.Now.ToString("MMMM dd, yyyy"),
                        description: description,
                        contractLink: contractLink
                    );

                    await emailService.SendAsync(
                        to: userEmail,
                        subject: $"New Contract Created - {contractNumber}",
                        htmlBody: htmlBody
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Send welcome email to new user
        /// </summary>
        public static async Task SendWelcomeEmailAsync(
            IEmailService emailService,
            string userName,
            string username,
            string userEmail,
            string role,
            string loginLink)
        {
            try
            {
                var htmlBody = EmailTemplateHelper.GetWelcomeEmail(
                    userName: userName,
                    username: username,
                    email: userEmail,
                    role: role,
                    loginLink: loginLink,
                    supportEmail: "support@company.com"
                );

                await emailService.SendAsync(
                    to: userEmail,
                    subject: "Welcome to ABB Monitoring System",
                    htmlBody: htmlBody
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
            }
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        public static async Task SendPasswordResetEmailAsync(
            IEmailService emailService,
            string userName,
            string userEmail,
            string resetLink)
        {
            try
            {
                var htmlBody = EmailTemplateHelper.GetPasswordResetEmail(
                    userName: userName,
                    resetLink: resetLink,
                    expirationTime: "24 hours",
                    securityEmail: "security@company.com"
                );

                await emailService.SendAsync(
                    to: userEmail,
                    subject: "Password Reset Request",
                    htmlBody: htmlBody
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send password reset email: {ex.Message}");
            }
        }
    }
}
```

---

### Metode 3: Direct Usage di Controller/Service

```csharp
public class SomeController : Controller
{
    private readonly IEmailService _emailService;

    public SomeController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<IActionResult> SendTestEmail()
    {
        // Method 1: Send simple email
        await _emailService.SendAsync(
            to: "user@example.com",
            subject: "Test Email",
            htmlBody: "<h1>Hello World</h1>"
        );

        // Method 2: Send dengan template
        var htmlBody = EmailTemplateHelper.GetNotificationEmail(
            title: "Test Notification",
            message: "This is a test notification",
            referenceId: "TEST-001",
            type: "Test",
            date: DateTime.Now.ToString("MMMM dd, yyyy"),
            actionLink: "https://yourapp.com",
            actionButtonText: "View Details"
        );

        await _emailService.SendAsync(
            to: "user@example.com",
            subject: "Test Notification",
            htmlBody: htmlBody
        );

        return Ok("Email sent!");
    }
}
```

---

## 💡 Contoh Implementasi Lengkap

### Update InvoiceService.cs

```csharp
public class InvoiceService : IInvoice
{
    private readonly ApplicationDBContext _context;
    private readonly INotifications _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        ApplicationDBContext context,
        INotifications notificationService,
        IEmailService emailService,
        ILogger<InvoiceService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<InvoiceDto> CreateAsync(InvoiceDto dto)
    {
        // Create invoice...
        var invoice = new Invoice
        {
            // ... set properties
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Get user and vendor info
        var user = await _context.Users.FindAsync(dto.CreatedByUserId);
        var vendor = await _context.Vendors.FindAsync(dto.VendorId);

        // Send in-app notification
        await _notificationService.CreateAsync(new NotificationDto
        {
            UserId = dto.CreatedByUserId,
            Title = "Invoice Created",
            Message = $"Invoice {invoice.InvoiceNumber} has been created successfully"
        });

        // Send email notification (async, non-blocking)
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var baseUrl = "https://yourapp.com"; // Or get from configuration
                    var htmlBody = EmailTemplateHelper.GetInvoiceCreatedEmail(
                        invoiceNumber: invoice.InvoiceNumber,
                        vendorName: vendor?.VendorName ?? "N/A",
                        amount: $"Rp {invoice.Amount:N0}",
                        dueDate: invoice.DueDate?.ToString("MMMM dd, yyyy") ?? "N/A",
                        status: "Pending",
                        createdBy: user.Username,
                        createdDate: DateTime.Now.ToString("MMMM dd, yyyy"),
                        description: invoice.Description ?? "No description",
                        invoiceLink: $"{baseUrl}/invoice/{invoice.InvoiceId}"
                    );

                    await _emailService.SendAsync(
                        to: user.Email,
                        subject: $"New Invoice Created - {invoice.InvoiceNumber}",
                        htmlBody: htmlBody
                    );

                    _logger.LogInformation("Email sent to {Email} for invoice {InvoiceNumber}",
                        user.Email, invoice.InvoiceNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email for invoice {InvoiceNumber}",
                        invoice.InvoiceNumber);
                }
            });
        }

        return invoice.ToDto();
    }
}
```

---

## 🧪 Testing Email

### 1. Test dengan Preview Controller

Browse ke:

- http://localhost:5170/api/test/email/preview/invoice
- http://localhost:5170/api/test/email/preview/contract

### 2. Test Kirim Email Real

Create test endpoint:

```csharp
[ApiController]
[Route("api/test/email")]
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailTestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-test")]
    public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            var htmlBody = EmailTemplateHelper.GetNotificationEmail(
                title: "Test Email",
                message: "This is a test email from Monitoring GS system",
                referenceId: "TEST-001",
                type: "Test",
                date: DateTime.Now.ToString("MMMM dd, yyyy HH:mm"),
                actionLink: "http://localhost:5170",
                actionButtonText: "Visit Dashboard"
            );

            await _emailService.SendAsync(
                to: request.Email,
                subject: "Test Email - ABB Monitoring System",
                htmlBody: htmlBody
            );

            return Ok(new { success = true, message = $"Test email sent to {request.Email}" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class TestEmailRequest
{
    public string Email { get; set; } = string.Empty;
}
```

Test via Postman/curl:

```bash
POST http://localhost:5170/api/test/email/send-test
Content-Type: application/json

{
  "email": "your-test-email@gmail.com"
}
```

---

## ⚠️ Troubleshooting

### Issue: Email tidak terkirim (Gmail)

**Solution:**

1. Enable "Less secure app access" (not recommended)
2. **OR** Use App Password:
   - Go to: https://myaccount.google.com/apppasswords
   - Generate new app password
   - Use this password di appsettings.json

### Issue: SMTP Connection Timeout

**Check:**

- Firewall blocking port 587/465
- SMTP host dan port correct
- SSL/TLS settings correct

### Issue: Authentication Failed

**Check:**

- Username/password correct
- Account allows SMTP access
- Two-factor authentication (use app password)

---

## 📝 Best Practices

1. **Error Handling**: Always wrap email sending dalam try-catch
2. **Async Non-Blocking**: Use `Task.Run()` untuk tidak block main process
3. **Logging**: Log email success/failure untuk debugging
4. **Testing**: Test dengan real email sebelum production
5. **Configuration**: Never hardcode credentials, use appsettings.json
6. **Rate Limiting**: Be aware of SMTP provider rate limits
7. **Retry Logic**: Implement retry untuk failed emails (optional)

---

## 🔒 Security Tips

1. **Never commit credentials** ke Git
2. Use **environment variables** atau Azure Key Vault untuk production
3. Use **App Passwords** untuk Gmail/Outlook
4. Enable **SSL/TLS** always
5. **Validate email addresses** sebelum send
6. **Rate limit** email sending untuk prevent abuse

---

**Ready to use!** 🚀
