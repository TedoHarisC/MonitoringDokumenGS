# Email Templates Guide

## Overview

Email templates yang kompatibel dengan **semua email providers** (Gmail, Outlook, Yahoo, Mailchimp, dll) menggunakan best practices:

- ✅ **Table-based layout** (bukan flexbox/grid)
- ✅ **Inline CSS** untuk kompatibilitas maksimal
- ✅ **Web-safe fonts** (Arial, Helvetica, sans-serif)
- ✅ **Maximum width 600px** untuk optimal viewing
- ✅ **XHTML 1.0 Transitional DOCTYPE**
- ✅ **Tested di Gmail, Outlook, Yahoo, Apple Mail**

## Template Files

### 1. BaseLayout.html

Template dasar untuk custom email content.

**Placeholders:**

- `{{EmailTitle}}` - Email title
- `{{CompanyName}}` - Company name (header)
- `{{EmailContent}}` - Main content HTML
- `{{Year}}` - Current year
- `{{CompanyAddress}}` - Company address
- `{{UnsubscribeLink}}`, `{{PrivacyLink}}`, `{{HelpLink}}`

### 2. Notification.html

General notification template dengan icon dan details box.

**Placeholders:**

- `{{NotificationTitle}}` - Title
- `{{NotificationMessage}}` - Message text
- `{{ReferenceId}}` - Reference ID
- `{{Type}}` - Notification type
- `{{Date}}` - Date
- `{{ActionLink}}` - CTA button link
- `{{ActionButtonText}}` - CTA button text
- `{{IconBackgroundColor}}` - Icon background color (hex)
- `{{Icon}}` - Icon emoji

**Colors:**

- Blue (Info): `#0d6efd`
- Green (Success): `#198754`
- Yellow (Warning): `#ffc107`
- Red (Error): `#dc3545`

### 3. InvoiceCreated.html

Dedicated template untuk invoice notifications.

**Placeholders:**

- `{{InvoiceNumber}}` - Invoice number
- `{{VendorName}}` - Vendor name
- `{{Amount}}` - Invoice amount
- `{{DueDate}}` - Due date
- `{{Status}}` - Invoice status
- `{{CreatedBy}}` - Creator name
- `{{CreatedDate}}` - Creation date
- `{{Description}}` - Invoice description
- `{{InvoiceLink}}` - Link to invoice details

**Design:** Blue theme dengan 📄 icon

### 4. ContractCreated.html

Dedicated template untuk contract notifications.

**Placeholders:**

- `{{ContractNumber}}` - Contract number
- `{{VendorName}}` - Vendor name
- `{{ContractValue}}` - Contract value
- `{{StartDate}}` - Start date
- `{{EndDate}}` - End date
- `{{Status}}` - Contract status
- `{{CreatedBy}}` - Creator name
- `{{CreatedDate}}` - Creation date
- `{{Description}}` - Contract description
- `{{ContractLink}}` - Link to contract details

**Design:** Green theme dengan 💼 icon, warning box

### 5. Welcome.html

Welcome email untuk new users.

**Placeholders:**

- `{{UserName}}` - User's full name
- `{{Username}}` - Username
- `{{Email}}` - Email address
- `{{Role}}` - User role
- `{{LoginLink}}` - Login page link
- `{{SupportEmail}}` - Support email

**Features:**

- Account details box
- Feature list dengan checkmarks
- Getting started guide
- Support information

### 6. PasswordReset.html

Password reset request email.

**Placeholders:**

- `{{UserName}}` - User's name
- `{{ResetLink}}` - Password reset link
- `{{ExpirationTime}}` - Link expiration time
- `{{SecurityEmail}}` - Security email

**Features:**

- Red theme untuk security emphasis
- Reset button + alternative link
- Expiration warning
- Security tips
- "Didn't request?" section

### 7. ApprovalRequired.html

Approval request notification.

**Placeholders:**

- `{{ApproverName}}` - Approver's name
- `{{ItemType}}` - Item type (Invoice/Contract)
- `{{ItemNumber}}` - Item number
- `{{Amount}}` - Amount/Value
- `{{SubmittedBy}}` - Submitter name
- `{{SubmittedDate}}` - Submission date
- `{{Priority}}` - Priority level
- `{{Notes}}` - Notes/Comments
- `{{ApproveLink}}` - Approve action link
- `{{RejectLink}}` - Reject action link
- `{{ViewDetailsLink}}` - View details link

**Features:**

- Yellow/warning theme
- Two action buttons (Approve/Reject)
- Priority indicator
- Action required notice

## Usage Examples

### Example 1: Send Invoice Notification

```csharp
using MonitoringDokumenGS.Services.Infrastructure;

// Using EmailTemplateHelper
var htmlBody = EmailTemplateHelper.GetInvoiceCreatedEmail(
    invoiceNumber: "INV-2026-001",
    vendorName: "PT. Vendor ABC",
    amount: "Rp 50,000,000",
    dueDate: "February 15, 2026",
    status: "Pending",
    createdBy: "John Doe",
    createdDate: "January 26, 2026",
    description: "Payment for services rendered in January 2026",
    invoiceLink: "https://yourapp.com/invoice/123"
);

await _emailService.SendAsync(
    to: "recipient@example.com",
    subject: "New Invoice Created - INV-2026-001",
    htmlBody: htmlBody
);
```

### Example 2: Send Contract Notification

```csharp
var htmlBody = EmailTemplateHelper.GetContractCreatedEmail(
    contractNumber: "CON-2026-005",
    vendorName: "PT. Supplier XYZ",
    contractValue: "Rp 250,000,000",
    startDate: "February 1, 2026",
    endDate: "December 31, 2026",
    status: "Draft",
    createdBy: "Jane Smith",
    createdDate: "January 26, 2026",
    description: "Annual maintenance contract for equipment",
    contractLink: "https://yourapp.com/contract/456"
);

await _emailService.SendAsync(
    to: "manager@example.com",
    subject: "New Contract Created - CON-2026-005",
    htmlBody: htmlBody
);
```

### Example 3: Send Welcome Email

```csharp
var htmlBody = EmailTemplateHelper.GetWelcomeEmail(
    userName: "John Doe",
    username: "johndoe",
    email: "john.doe@example.com",
    role: "Admin",
    loginLink: "https://yourapp.com/auth/login",
    supportEmail: "support@yourcompany.com"
);

await _emailService.SendAsync(
    to: "john.doe@example.com",
    subject: "Welcome to ABB Monitoring System",
    htmlBody: htmlBody
);
```

### Example 4: Send Password Reset

```csharp
var resetToken = GeneratePasswordResetToken(); // Your token generation
var resetLink = $"https://yourapp.com/auth/reset-password?token={resetToken}";

var htmlBody = EmailTemplateHelper.GetPasswordResetEmail(
    userName: "John Doe",
    resetLink: resetLink,
    expirationTime: "24 hours",
    securityEmail: "security@yourcompany.com"
);

await _emailService.SendAsync(
    to: "john.doe@example.com",
    subject: "Password Reset Request",
    htmlBody: htmlBody
);
```

### Example 5: Send Approval Request

```csharp
var htmlBody = EmailTemplateHelper.GetApprovalRequiredEmail(
    approverName: "Manager Name",
    itemType: "Invoice",
    itemNumber: "INV-2026-001",
    amount: "Rp 50,000,000",
    submittedBy: "John Doe",
    submittedDate: "January 26, 2026",
    priority: "High",
    notes: "Urgent payment required for vendor",
    approveLink: "https://yourapp.com/approval/123/approve",
    rejectLink: "https://yourapp.com/approval/123/reject",
    viewDetailsLink: "https://yourapp.com/invoice/123"
);

await _emailService.SendAsync(
    to: "manager@example.com",
    subject: "Approval Required: INV-2026-001",
    htmlBody: htmlBody
);
```

### Example 6: General Notification

```csharp
var htmlBody = EmailTemplateHelper.GetNotificationEmail(
    title: "Document Uploaded",
    message: "A new document has been uploaded to your account",
    referenceId: "DOC-2026-789",
    type: "Document",
    date: DateTime.Now.ToString("MMMM dd, yyyy"),
    actionLink: "https://yourapp.com/documents/789",
    actionButtonText: "View Document",
    iconBackgroundColor: "#0d6efd",
    icon: "📎"
);

await _emailService.SendAsync(
    to: "user@example.com",
    subject: "New Document Uploaded",
    htmlBody: htmlBody
);
```

### Example 7: Custom Email dengan Base Layout

```csharp
var customContent = @"
<h2 style='color: #212529; font-family: Arial, Helvetica, sans-serif;'>
    Custom Title
</h2>
<p style='color: #495057; font-family: Arial, Helvetica, sans-serif; line-height: 24px;'>
    Your custom HTML content here...
</p>
";

var htmlBody = EmailTemplateHelper.GetBaseLayoutEmail(
    emailTitle: "Custom Email",
    companyName: "ABB Monitoring",
    emailContent: customContent,
    year: DateTime.Now.Year.ToString(),
    companyAddress: "Jakarta, Indonesia",
    unsubscribeLink: "https://yourapp.com/unsubscribe",
    privacyLink: "https://yourapp.com/privacy",
    helpLink: "https://yourapp.com/help"
);

await _emailService.SendAsync(
    to: "user@example.com",
    subject: "Custom Email",
    htmlBody: htmlBody
);
```

## Integration dengan NotificationHelper

Integrate email notifications dengan NotificationHelper:

```csharp
// In NotificationHelper.cs
public static async Task NotifyInvoiceCreatedWithEmailAsync(
    INotifications notificationService,
    IEmailService emailService,
    Guid userId,
    string userEmail,
    string userName,
    string invoiceNumber,
    string vendorName,
    string amount,
    string dueDate)
{
    // Create in-app notification
    await NotifyInvoiceCreatedAsync(notificationService, userId, invoiceNumber);

    // Send email notification
    var htmlBody = EmailTemplateHelper.GetInvoiceCreatedEmail(
        invoiceNumber: invoiceNumber,
        vendorName: vendorName,
        amount: amount,
        dueDate: dueDate,
        status: "Pending",
        createdBy: userName,
        createdDate: DateTime.Now.ToString("MMMM dd, yyyy"),
        description: $"New invoice from {vendorName}",
        invoiceLink: $"https://yourapp.com/invoice/{invoiceNumber}"
    );

    await emailService.SendAsync(
        to: userEmail,
        subject: $"New Invoice Created - {invoiceNumber}",
        htmlBody: htmlBody
    );
}
```

## Testing Email Templates

### Method 1: Online Email Testing Tools

1. **Litmus** (https://litmus.com)
   - Test di 90+ email clients
   - Preview tampilan di berbagai devices

2. **Email on Acid** (https://www.emailonacid.com)
   - Comprehensive testing
   - Spam filter testing

3. **Mailtrap** (https://mailtrap.io)
   - Development email testing
   - Free tier available

### Method 2: Manual Testing

```csharp
// Test controller untuk preview email templates
[ApiController]
[Route("api/test/email")]
public class EmailTestController : ControllerBase
{
    [HttpGet("preview/{template}")]
    public IActionResult PreviewTemplate(string template)
    {
        string html = template.ToLower() switch
        {
            "invoice" => EmailTemplateHelper.GetInvoiceCreatedEmail(
                "INV-TEST-001", "Test Vendor", "Rp 1,000,000",
                "Dec 31, 2026", "Pending", "Test User",
                "Jan 26, 2026", "Test invoice", "#"),

            "contract" => EmailTemplateHelper.GetContractCreatedEmail(
                "CON-TEST-001", "Test Vendor", "Rp 5,000,000",
                "Jan 1, 2026", "Dec 31, 2026", "Active",
                "Test User", "Jan 26, 2026", "Test contract", "#"),

            "welcome" => EmailTemplateHelper.GetWelcomeEmail(
                "Test User", "testuser", "test@example.com",
                "Admin", "#"),

            "reset" => EmailTemplateHelper.GetPasswordResetEmail(
                "Test User", "#"),

            _ => "<h1>Unknown template</h1>"
        };

        return Content(html, "text/html");
    }
}
```

Test URLs:

- http://localhost:5170/api/test/email/preview/invoice
- http://localhost:5170/api/test/email/preview/contract
- http://localhost:5170/api/test/email/preview/welcome
- http://localhost:5170/api/test/email/preview/reset

## Email Compatibility

### ✅ Tested & Working On:

1. **Gmail** (Desktop & Mobile)
   - Web interface
   - iOS app
   - Android app

2. **Outlook** (Desktop & Mobile)
   - Outlook.com web
   - Outlook Windows app
   - Outlook Mac app
   - Outlook iOS/Android

3. **Yahoo Mail**
   - Web interface
   - Mobile apps

4. **Apple Mail**
   - macOS Mail app
   - iOS Mail app

5. **Mailchimp**
   - Campaign preview
   - Template import

6. **Thunderbird**
   - Desktop client

### CSS Support Limitations

**Supported:**

- ✅ Table layouts
- ✅ Inline styles
- ✅ Background colors
- ✅ Border-radius (with fallbacks)
- ✅ Padding/Margin
- ✅ Font properties
- ✅ Colors (hex, rgb)

**Not Supported (Avoided):**

- ❌ Flexbox
- ❌ Grid
- ❌ Position: absolute/fixed
- ❌ External CSS files
- ❌ JavaScript
- ❌ Forms (limited support)
- ❌ Videos
- ❌ CSS animations

## Best Practices

### 1. Always Use Tables for Layout

```html
<table border="0" cellpadding="0" cellspacing="0" width="600">
  <tr>
    <td>Content here</td>
  </tr>
</table>
```

### 2. Inline All CSS

```html
<td style="padding: 20px; background-color: #f8f9fa; color: #212529;"></td>
```

### 3. Use Web-Safe Fonts

```css
font-family: Arial, Helvetica, sans-serif;
```

### 4. Set Image Properties

```html
<img
  src="image.jpg"
  alt="Description"
  width="200"
  height="100"
  style="border: 0; display: block;"
/>
```

### 5. Use Hex Colors

```css
color: #212529; /* Good */
color: rgb(33, 37, 41); /* Also OK */
color: rgba(33, 37, 41, 0.8); /* Limited support */
```

### 6. Test Button Links

```html
<table border="0" cellpadding="0" cellspacing="0">
  <tr>
    <td style="border-radius: 6px; background-color: #0d6efd;">
      <a
        href="{{Link}}"
        target="_blank"
        style="display: inline-block; padding: 14px 40px; 
                      color: #ffffff; text-decoration: none; 
                      font-weight: bold;"
      >
        Button Text
      </a>
    </td>
  </tr>
</table>
```

### 7. Mobile Responsive

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
```

## Troubleshooting

### Issue: Images tidak tampil

**Solution:**

- Use absolute URLs untuk images
- Host images di CDN/web server
- Set proper MIME types

### Issue: Layout broken di Outlook

**Solution:**

- Use tables, avoid floats
- Set explicit widths
- Add `mso-` specific styles for Outlook

### Issue: Fonts tidak match

**Solution:**

- Stick to web-safe fonts
- Provide fallback fonts
- Arial, Helvetica, sans-serif adalah safest

### Issue: Colors berbeda

**Solution:**

- Use hex colors (#RRGGBB)
- Avoid gradients (atau provide fallback)
- Test di multiple clients

## Files Structure

```
EmailTemplates/
├── BaseLayout.html              // Base template
├── Notification.html            // General notification
├── InvoiceCreated.html          // Invoice specific
├── ContractCreated.html         // Contract specific
├── Welcome.html                 // Welcome new user
├── PasswordReset.html           // Password reset
└── ApprovalRequired.html        // Approval request

Services/Infrastructure/
└── EmailTemplateHelper.cs       // Helper class dengan methods
```

## Future Enhancements

1. 📧 **Multi-language Support** - Template dalam berbagai bahasa
2. 🎨 **Custom Themes** - Allow custom color schemes
3. 📊 **Email Analytics** - Track open rates, click rates
4. 🔄 **Template Versioning** - Version control untuk templates
5. 📝 **Template Editor** - Web-based WYSIWYG editor
6. 🧪 **A/B Testing** - Test different template variants
7. 📱 **SMS Integration** - Send SMS notifications
8. 🔔 **Push Notifications** - Browser push notifications

---

**Created**: January 26, 2026  
**Last Updated**: January 26, 2026  
**Version**: 1.0  
**Compatibility**: All major email clients tested ✅
