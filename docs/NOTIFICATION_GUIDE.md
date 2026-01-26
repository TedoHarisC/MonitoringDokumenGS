# Notification System - Usage Guide

## Overview

Sistem notifikasi sudah **per-user**, setiap user hanya bisa melihat notifikasi milik mereka sendiri.

## Cara Kerja

### 1. Auto-Notification (Sudah Terintegrasi)

Notifikasi otomatis dibuat saat:

#### Invoice

- **Create Invoice** → Notification dikirim ke user yang membuat invoice
- Message: "Invoice {InvoiceNumber} has been successfully created with amount {Amount}"

#### Contract

- **Create Contract** → Notification dikirim ke user yang membuat contract
- Message: "Contract {ContractNumber} has been successfully created from {StartDate} to {EndDate}"

### 2. Manual Notification (via API)

```http
POST /api/notifications
Authorization: Bearer {token}
Content-Type: application/json

{
    "userId": "guid-user-id",
    "title": "Custom Title",
    "message": "Your custom message here"
}
```

### 3. Programmatic Notification (dalam Service)

#### Cara 1: Direct Service Call

```csharp
public class YourService
{
    private readonly INotifications _notificationService;

    public YourService(INotifications notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task DoSomething(Guid userId)
    {
        // Your logic here...

        // Send notification
        await _notificationService.CreateAsync(new NotificationDto
        {
            UserId = userId,
            Title = "Action Completed",
            Message = "Your action has been completed successfully"
        });
    }
}
```

#### Cara 2: Menggunakan NotificationHelper (Recommended)

```csharp
public class YourService
{
    private readonly NotificationHelper _notificationHelper;

    public YourService(INotifications notificationService)
    {
        _notificationHelper = new NotificationHelper(notificationService);
    }

    public async Task DoSomething(Guid userId)
    {
        // Predefined templates
        await _notificationHelper.NotifyInvoiceCreatedAsync(userId, "INV-001", 1000000);
        await _notificationHelper.NotifyContractApprovedAsync(userId, "CTR-001");

        // Custom notification
        await _notificationHelper.SendNotificationAsync(
            userId,
            "Custom Title",
            "Custom message"
        );

        // Multiple users
        var userIds = new[] { userId1, userId2, userId3 };
        await _notificationHelper.SendNotificationToMultipleUsersAsync(
            userIds,
            "Team Update",
            "New project has been assigned"
        );
    }
}
```

## Template Notifications Available

NotificationHelper menyediakan method template:

1. **Invoice**
   - `NotifyInvoiceCreatedAsync()` - Invoice created
   - `NotifyInvoiceUpdatedAsync()` - Invoice updated
   - `NotifyInvoiceApprovedAsync()` - Invoice approved

2. **Contract**
   - `NotifyContractCreatedAsync()` - Contract created
   - `NotifyContractUpdatedAsync()` - Contract updated
   - `NotifyContractApprovedAsync()` - Contract approved

3. **Attachment**
   - `NotifyAttachmentUploadedAsync()` - File uploaded

4. **Custom**
   - `SendNotificationAsync()` - Custom notification
   - `SendNotificationToMultipleUsersAsync()` - Broadcast ke multiple users

## Frontend Integration

Notification otomatis muncul di header dashboard:

- Auto-refresh setiap 30 detik
- Badge counter untuk unread notifications
- Click notification untuk mark as read
- "Mark All as Read" button

## Best Practices

1. **Jangan fail main operation** jika notification gagal

   ```csharp
   try
   {
       await _notificationService.CreateAsync(...);
   }
   catch (Exception ex)
   {
       // Log error tapi jangan throw
       Console.WriteLine($"Notification failed: {ex.Message}");
   }
   ```

2. **Kirim ke user yang relevan**
   - Creator: saat create/update
   - Approver: saat pending approval
   - All team: untuk broadcast message

3. **Message yang jelas dan actionable**
   - ❌ "Something happened"
   - ✅ "Invoice INV-001 has been approved by Manager"

4. **Gunakan template yang sudah ada** dari NotificationHelper untuk consistency

## Example Use Cases

### Approval Workflow

```csharp
// Saat invoice di-submit untuk approval
await _notificationHelper.SendNotificationAsync(
    approverId,
    "Approval Required",
    $"Invoice {invoiceNumber} needs your approval (Amount: {amount:N2})"
);

// Saat invoice di-approve
await _notificationHelper.NotifyInvoiceApprovedAsync(creatorUserId, invoiceNumber);
```

### File Upload

```csharp
// Setelah file berhasil diupload
await _notificationHelper.NotifyAttachmentUploadedAsync(
    userId,
    fileName,
    "Invoices"
);
```

### Team Notification

```csharp
// Broadcast ke team
var teamUserIds = await GetTeamUserIds();
await _notificationHelper.SendNotificationToMultipleUsersAsync(
    teamUserIds,
    "New Project",
    "New project XYZ has been created and assigned to the team"
);
```
