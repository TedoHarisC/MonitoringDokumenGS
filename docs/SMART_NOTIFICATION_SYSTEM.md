# 🔔 Smart Notification System - Complete Guide

## 📋 Overview

Sistem notifikasi otomatis dengan **anti-spam mechanism** untuk Budget Overbudget dan Contract Expiring reminders.

## 🎯 Features Implemented

### ✅ 1. Budget Overbudget Alert

**Trigger Strategy: Real-time + Daily Scheduled**

#### **Real-time Trigger** ⚡

- Dipicu otomatis saat:
  - Invoice baru dibuat (`CreateAsync`)
  - Invoice diupdate dengan perubahan amount/month/year (`UpdateAsync`)
- Langsung cek budget dan kirim alert jika overbudget
- Tidak mengganggu proses invoice (non-blocking)

#### **Scheduled Trigger** 📅

- Berjalan otomatis setiap hari jam **09:00 AM**
- Double-check untuk keamanan
- Backup untuk edge cases

#### **Alert Levels:**

| Level        | Condition        | Priority | Icon |
| ------------ | ---------------- | -------- | ---- |
| **Critical** | Over 100% budget | CRITICAL | ⚠️   |
| **Warning**  | 90-100% budget   | WARNING  | ⚡   |

#### **Anti-Spam:**

- ✅ Maximum 1 alert per kondisi per 24 jam
- ✅ Tracking via `SYS_NotificationLog`
- ✅ Cooldown period: 24 hours

---

### ✅ 2. Contract Expiring Reminder

**Trigger Strategy: Multi-Level Reminders**

#### **Scheduled Trigger** 📅

- Berjalan otomatis setiap hari jam **08:00 AM**
- Cek semua contract yang mendekati expired

#### **Reminder Levels:**

| Days Left   | Level | Priority | Icon | Frequency                |
| ----------- | ----- | -------- | ---- | ------------------------ |
| **60 days** | 1     | LOW      | 📋   | Once (3 days cooldown)   |
| **30 days** | 2     | MEDIUM   | 📅   | Once (3 days cooldown)   |
| **14 days** | 3     | HIGH     | ⏰   | Once (3 days cooldown)   |
| **7 days**  | 4     | URGENT   | ⚠️   | Once (3 days cooldown)   |
| **≤3 days** | 5     | CRITICAL | 🚨   | **Daily** (24h cooldown) |

#### **Recipients:**

- **Primary:** Contract PIC (creator)
- **Secondary (≤14 days):** Admins + Super Admins

#### **Anti-Spam:**

- ✅ Track reminder level (tidak kirim ulang level yang sama)
- ✅ Progressive reminders (level naik seiring waktu)
- ✅ Daily reminder hanya untuk 3 hari terakhir
- ✅ Cooldown: 72 hours (normal), 24 hours (critical)

---

## 🛠️ Technical Architecture

### **Services Created:**

1. **NotificationLog Model** (`Models/Infrastructure/NotificationLog.cs`)
   - Tracking notification yang sudah dikirim
   - Anti-spam mechanism

2. **INotificationLog Service** (`Services/Infrastructure/NotificationLogService.cs`)
   - `CanSendNotificationAsync()` - Check cooldown
   - `LogNotificationSentAsync()` - Record notification
   - `GetContractReminderLevelAsync()` - Track contract reminder progress

3. **Updated BudgetNotificationJob** (`Services/Notification/BudgetNotificationJob.cs`)
   - Anti-spam checks
   - Real-time method: `CheckBudgetOnInvoiceChangeAsync()`
   - Scheduled method: `RunAsync()`

4. **Updated ContractNotificationJob** (`Services/Notification/ContractNotification.cs`)
   - Multi-level reminder system
   - Anti-spam checks
   - Progressive urgency levels

5. **Background Services:**
   - `BudgetCheckBackgroundService.cs` - Daily 9 AM
   - `ContractExpiringBackgroundService.cs` - Daily 8 AM

6. **Updated InvoiceService** (`Services/Transaction/InvoiceService.cs`)
   - Real-time budget check trigger

---

## 📊 Database Schema

### **SYS_NotificationLog Table**

```sql
CREATE TABLE [SYS_NotificationLog] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [NotificationType] NVARCHAR(100) NOT NULL,
    [ReferenceId] NVARCHAR(100) NOT NULL,
    [RecipientEmail] NVARCHAR(255) NOT NULL,
    [ReminderLevel] INT NOT NULL DEFAULT 0,
    [SentAt] DATETIME2 NOT NULL,
    [Details] NVARCHAR(500),
    [IsDeleted] BIT NOT NULL DEFAULT 0
);

-- Indexes for performance
CREATE INDEX IX_SYS_NotificationLog_Type_Ref_SentAt
    ON SYS_NotificationLog (NotificationType, ReferenceId, SentAt);

CREATE INDEX IX_SYS_NotificationLog_ReferenceId
    ON SYS_NotificationLog (ReferenceId);
```

### **Notification Types:**

```csharp
// Budget
BUDGET_OVERBUDGET_MONTHLY
BUDGET_OVERBUDGET_YEARLY
BUDGET_WARNING_MONTHLY

// Contract
CONTRACT_EXPIRING_60DAYS   // Level 1
CONTRACT_EXPIRING_30DAYS   // Level 2
CONTRACT_EXPIRING_14DAYS   // Level 3
CONTRACT_EXPIRING_7DAYS    // Level 4
CONTRACT_EXPIRING_FINAL    // Level 5 (≤3 days)
```

---

## 🚀 How It Works

### **Budget Alert Flow:**

```
Invoice Created/Updated
    ↓
CheckBudgetOnInvoiceChangeAsync()
    ↓
CanSendNotificationAsync() → Check cooldown (24h)
    ↓
    ├─ If cooldown active → Skip
    └─ If cooldown expired → Send alert
        ↓
        Send Email + In-app Notification
        ↓
        LogNotificationSentAsync() → Record in DB
```

### **Contract Reminder Flow:**

```
Daily 8 AM Trigger
    ↓
Get all expiring contracts
    ↓
For each contract:
    ├─ Calculate days left
    ├─ Determine reminder level
    ├─ Get last reminder level sent
    ├─ If new level > last level
    │   ↓
    │   CanSendNotificationAsync() → Check cooldown
    │       ↓
    │       ├─ If OK → Send notification
    │       └─ LogNotificationSentAsync()
    └─ Else skip
```

---

## 🔧 Configuration

### **Scheduler Timing:**

Edit di background service files jika ingin ubah jam:

```csharp
// BudgetCheckBackgroundService.cs
private readonly TimeSpan _checkTime = new TimeSpan(9, 0, 0); // 9:00 AM

// ContractExpiringBackgroundService.cs
private readonly TimeSpan _checkTime = new TimeSpan(8, 0, 0); // 8:00 AM
```

### **Cooldown Period:**

```csharp
// Budget alerts: 24 hours
await _notificationLog.CanSendNotificationAsync(notificationType, referenceId, cooldownHours: 24);

// Contract: 72 hours (normal), 24 hours (critical)
int cooldownHours = daysLeft <= 3 ? 24 : 72;
```

---

## 📱 API Endpoints (Manual Trigger)

Tetap bisa trigger manual via API:

### **Budget Check**

```bash
POST /api/jobs/budget-overbudget
Authorization: Bearer {token}
Role: SUPER_ADMIN, ADMIN
```

### **Contract Check**

```bash
POST /api/jobs/contract-expiry
Authorization: Bearer {token}
Role: SUPER_ADMIN, ADMIN
```

---

## 📈 Monitoring & Logs

### **Check Notification History:**

```sql
-- Budget alerts sent today
SELECT * FROM SYS_NotificationLog
WHERE NotificationType LIKE 'BUDGET%'
  AND SentAt >= CAST(GETDATE() AS DATE)
  AND IsDeleted = 0
ORDER BY SentAt DESC;

-- Contract reminders by level
SELECT
    NotificationType,
    ReminderLevel,
    COUNT(*) as Count,
    MAX(SentAt) as LastSent
FROM SYS_NotificationLog
WHERE NotificationType LIKE 'CONTRACT%'
  AND IsDeleted = 0
GROUP BY NotificationType, ReminderLevel
ORDER BY ReminderLevel;
```

### **Application Logs:**

Background services akan log:

- Start/stop events
- Next scheduled run time
- Success/error pada setiap execution
- Anti-spam blocks

Cek di console output atau logging provider yang digunakan.

---

## ✅ Testing Checklist

### **Budget Alerts:**

- [ ] Create invoice → Real-time alert jika overbudget
- [ ] Update invoice amount → Real-time check
- [ ] Update invoice month/year → Check both old & new period
- [ ] Daily scheduler berjalan jam 9 AM
- [ ] Alert tidak spam (max 1x per 24 jam per kondisi)
- [ ] Email diterima admin
- [ ] In-app notification muncul

### **Contract Reminders:**

- [ ] Contract 60 days → Level 1 reminder
- [ ] Contract 30 days → Level 2 reminder
- [ ] Contract 14 days → Level 3 reminder + admin notified
- [ ] Contract 7 days → Level 4 reminder
- [ ] Contract ≤3 days → Daily reminder (Level 5)
- [ ] Tidak kirim duplicate level
- [ ] Daily scheduler berjalan jam 8 AM
- [ ] Email diterima PIC & admins

### **Anti-Spam:**

- [ ] Budget alert cooldown 24 jam
- [ ] Contract reminder tidak repeat level sama
- [ ] Contract critical (≤3 days) kirim daily
- [ ] `SYS_NotificationLog` table tercreate
- [ ] Notification logs tersimpan

---

## 🎨 UX Benefits

| Before                    | After                                   |
| ------------------------- | --------------------------------------- |
| Manual check budget       | ✅ Real-time alert + daily backup       |
| No contract reminders     | ✅ Progressive 5-level reminders        |
| Risk of notification spam | ✅ Smart anti-spam mechanism            |
| Miss critical deadlines   | ✅ Daily reminders for urgent items     |
| One-size-fits-all         | ✅ Priority-based notification strategy |

---

## 🔐 Security & Performance

- ✅ Background services isolated (CreateScope pattern)
- ✅ Non-blocking real-time checks (try-catch)
- ✅ Database indexed for query performance
- ✅ Role-based notification recipients
- ✅ Graceful error handling
- ✅ Logging for audit trail

---

## 📝 Migration Steps

1. **Apply Migration:**

   ```bash
   dotnet ef database update
   ```

2. **Restart Application:**
   Background services akan auto-start

3. **Verify Logs:**
   Check console untuk:
   - "Budget Check Background Service started"
   - "Contract Expiring Background Service started"
   - Scheduled times

4. **Test Real-time:**
   Create/update invoice dan cek notification

5. **Monitor Daily:**
   Tunggu jam 8 AM & 9 AM untuk scheduler

---

## 🎯 Summary

### **Budget Alerts:**

- ⚡ Real-time saat invoice change
- 📅 Daily check jam 9 AM
- 🛡️ Anti-spam: 1x per 24 jam

### **Contract Reminders:**

- 📅 Daily check jam 8 AM
- 📊 5-level progressive reminders
- 🛡️ Anti-spam: Track reminder level + cooldown

### **Anti-Spam System:**

- 📝 `SYS_NotificationLog` tracking
- ⏱️ Cooldown mechanism
- 🎚️ Reminder level progression

**Result:** Zero spam, timely notifications, optimal UX! 🎉
