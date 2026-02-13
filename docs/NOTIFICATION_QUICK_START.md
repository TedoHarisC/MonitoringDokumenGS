# 🚀 Quick Start - Smart Notification System

## 📦 What's Implemented

### ✅ Budget Overbudget Alert

- **Real-time:** Langsung cek saat invoice dibuat/diupdate
- **Scheduled:** Auto-check setiap hari jam 9 pagi
- **Anti-spam:** Max 1 alert per 24 jam untuk setiap kondisi

### ✅ Contract Expiring Reminder

- **Scheduled:** Auto-check setiap hari jam 8 pagi
- **Multi-level:** 5 tingkat reminder (60, 30, 14, 7, ≤3 hari)
- **Anti-spam:** Progressive reminders, daily untuk 3 hari terakhir

---

## 🔨 Setup Steps

### 1️⃣ Run Migration

```bash
dotnet ef database update
```

Atau kalau migration belum dibuat:

```bash
dotnet ef migrations add AddNotificationLogTable
dotnet ef database update
```

### 2️⃣ Restart Application

```bash
dotnet run
```

Background services akan otomatis start dan log:

```
info: BudgetCheckBackgroundService[0]
      Budget Check Background Service started
      Budget check scheduled for 2026-02-14 09:00:00

info: ContractExpiringBackgroundService[0]
      Contract Expiring Background Service started
      Contract expiring check scheduled for 2026-02-14 08:00:00
```

### 3️⃣ Verify Database

```sql
-- Check table created
SELECT * FROM SYS_NotificationLog;
```

---

## 📊 How to Test

### Test Budget Alert (Real-time)

1. Login sebagai user
2. Create invoice atau update invoice amount
3. Jika menyebabkan overbudget → Alert langsung dikirim
4. Check:
   - Email admin
   - In-app notification
   - Database log

```sql
-- Check budget alerts
SELECT * FROM SYS_NotificationLog
WHERE NotificationType LIKE 'BUDGET%'
ORDER BY SentAt DESC;
```

### Test Contract Reminder

Option 1: **Wait for scheduler** (8 AM besok)

Option 2: **Manual trigger via API**

```bash
# Get token first (login)
POST /api/auth/login

# Then trigger
POST /api/jobs/contract-expiry
Authorization: Bearer {your-token}
```

Check:

- Email ke PIC contract
- Email ke admin (untuk contract ≤14 hari)
- In-app notifications

```sql
-- Check contract reminders
SELECT * FROM SYS_NotificationLog
WHERE NotificationType LIKE 'CONTRACT%'
ORDER BY SentAt DESC;
```

---

## 🎯 Expected Behavior

### Budget Alert

| Scenario                           | Result                |
| ---------------------------------- | --------------------- |
| Invoice baru → overbudget          | ✅ Alert immediate    |
| Update invoice amount → overbudget | ✅ Alert immediate    |
| Alert sama dalam 24 jam            | ⛔ Blocked (cooldown) |
| Daily check jam 9 AM               | ✅ Auto-run           |

### Contract Reminder

| Days to Expiry | Level       | Sent To     | Frequency |
| -------------- | ----------- | ----------- | --------- |
| 60 days        | 1️⃣ LOW      | PIC         | Once      |
| 30 days        | 2️⃣ MEDIUM   | PIC         | Once      |
| 14 days        | 3️⃣ HIGH     | PIC + Admin | Once      |
| 7 days         | 4️⃣ URGENT   | PIC + Admin | Once      |
| ≤3 days        | 5️⃣ CRITICAL | PIC + Admin | Daily     |

---

## 🛠️ Manual Testing Commands

### Check Background Services Status

```bash
# Logs akan tampil di console
# Cari:
# - "Background Service started"
# - "scheduled for"
# - "completed successfully"
```

### Manual Trigger Budget Check

```bash
POST http://localhost:5008/api/jobs/budget-overbudget
Authorization: Bearer {token}
Content-Type: application/json
```

### Manual Trigger Contract Check

```bash
POST http://localhost:5008/api/jobs/contract-expiry
Authorization: Bearer {token}
Content-Type: application/json
```

### Check Last Run

```sql
-- Budget alerts hari ini
SELECT TOP 10 *
FROM SYS_NotificationLog
WHERE NotificationType LIKE 'BUDGET%'
  AND SentAt >= CAST(GETDATE() AS DATE)
ORDER BY SentAt DESC;

-- Contract reminders hari ini
SELECT TOP 10 *
FROM SYS_NotificationLog
WHERE NotificationType LIKE 'CONTRACT%'
  AND SentAt >= CAST(GETDATE() AS DATE)
ORDER BY SentAt DESC;

-- Summary by type
SELECT
    NotificationType,
    COUNT(*) as TotalSent,
    MAX(SentAt) as LastSent,
    COUNT(DISTINCT RecipientEmail) as Recipients
FROM SYS_NotificationLog
WHERE IsDeleted = 0
GROUP BY NotificationType
ORDER BY NotificationType;
```

---

## 📝 Troubleshooting

### Problem: Background service tidak start

**Check:** Application startup logs

```
// Harus ada log ini:
Budget Check Background Service started
Contract Expiring Background Service started
```

**Solution:** Verify `Program.cs` registered services:

```csharp
builder.Services.AddHostedService<BudgetCheckBackgroundService>();
builder.Services.AddHostedService<ContractExpiringBackgroundService>();
```

### Problem: Real-time budget check tidak jalan

**Check:** Invoice creation/update logs

**Solution:** Verify `InvoiceService` inject `IBudgetNotificationJob`

### Problem: Notification spam

**Check database:**

```sql
-- Check duplicate notifications
SELECT
    NotificationType,
    ReferenceId,
    COUNT(*) as Count,
    MIN(SentAt) as FirstSent,
    MAX(SentAt) as LastSent,
    DATEDIFF(HOUR, MIN(SentAt), MAX(SentAt)) as HoursDiff
FROM SYS_NotificationLog
WHERE NotificationType LIKE 'BUDGET%'
  AND IsDeleted = 0
GROUP BY NotificationType, ReferenceId
HAVING COUNT(*) > 1
ORDER BY Count DESC;
```

If seeing duplicates within 24h → Check `CanSendNotificationAsync()` logic

### Problem: Email tidak terkirim

1. Check email configuration di `appsettings.json`
2. Check SMTP settings
3. Check recipient email valid
4. Check application logs untuk SMTP errors

---

## 🎨 Customization

### Change Scheduler Time

Edit files:

- `BudgetCheckBackgroundService.cs` - Line ~14
- `ContractExpiringBackgroundService.cs` - Line ~14

```csharp
// Change from 9 AM to 10 AM
private readonly TimeSpan _checkTime = new TimeSpan(10, 0, 0);
```

### Change Cooldown Period

Edit `BudgetNotificationJob.cs` or `ContractNotification.cs`:

```csharp
// Budget: from 24h to 48h
await _notificationLog.CanSendNotificationAsync(
    notificationType,
    referenceId,
    cooldownHours: 48);  // Changed from 24

// Contract critical: from 24h to 12h
int cooldownHours = daysLeft <= 3 ? 12 : 72;  // Changed from 24
```

### Change Contract Reminder Thresholds

Edit `ContractNotification.cs` - `RunAsync()` method:

```csharp
// Add 45-day reminder (between 60 and 30):
else if (daysLeft <= 45)
{
    notificationType = "CONTRACT_EXPIRING_45DAYS";
    reminderLevel = 2;  // Shift other levels
    urgencyLevel = "MEDIUM";
}
```

---

## ✅ Success Indicators

### After Implementation:

- [x] Migration applied successfully
- [x] Background services started
- [x] Budget alerts sent on invoice change
- [x] Contract reminders scheduled
- [x] No spam notifications
- [x] Emails received by admins/PICs
- [x] In-app notifications visible
- [x] Logs recorded in database

### Daily Operations:

- Budget check runs at 9 AM
- Contract check runs at 8 AM
- Notification logs growing in database
- No duplicate alerts within cooldown
- Users receiving timely notifications

---

## 📞 Support

For issues or questions:

1. Check application logs
2. Query `SYS_NotificationLog` table
3. Verify email configuration
4. Review [SMART_NOTIFICATION_SYSTEM.md](SMART_NOTIFICATION_SYSTEM.md) for details

---

**Implementation Date:** February 13, 2026
**Status:** ✅ Production Ready
**Anti-Spam:** ✅ Fully Implemented
