# Budget Overbudget Alert System

## 📊 Overview

System ini secara otomatis mendeteksi ketika spending melebihi budget (overbudget) dan mengirim notifikasi ke admin melalui:

- ✅ In-app notification
- ✅ Email alert
- ✅ Audit log

## 🎯 Kapan Alert Dikirim?

### 1. **Overbudget Alert (Critical)** 🚨

Alert dikirim ketika:

- **Monthly Budget**: Spending bulan ini > Monthly Budget
- **Yearly Budget**: Total spending tahun ini > Total Budget

**Notifikasi:**

- Icon: ⚠️ (Red)
- Subject: "BUDGET OVERRUN ALERT"
- Priority: **CRITICAL**
- Recipients: Super Admin & Admin

### 2. **Budget Warning (Warning)** ⚡

Alert dikirim ketika:

- Budget utilization >= 90% dan <= 100%
- Hanya untuk bulan berjalan

**Notifikasi:**

- Icon: ⚡ (Yellow)
- Subject: "Budget Warning"
- Priority: **Warning**
- Recipients: Super Admin & Admin

## 🔧 Cara Menggunakan

### A. Manual Trigger (via API)

#### 1. **Run Budget Check**

```bash
POST /api/jobs/budget-overbudget
Authorization: Bearer {token}
Role: SUPER_ADMIN atau ADMIN
```

**Response:**

```json
{
  "success": true,
  "message": "Budget overbudget check executed successfully."
}
```

#### 2. **Get Budget Status**

```bash
GET /api/jobs/budget-status/{year}
Authorization: Bearer {token}
Role: SUPER_ADMIN atau ADMIN
```

**Response:**

```json
{
  "success": true,
  "data": {
    "year": 2026,
    "totalBudget": 120000000,
    "monthlyBudget": 10000000,
    "totalSpent": 85000000,
    "remainingBudget": 35000000,
    "budgetUtilizationPercent": 70.83,
    "isOverBudget": false,
    "monthlyStatus": [
      {
        "month": 1,
        "monthName": "January",
        "budget": 10000000,
        "spent": 12500000,
        "remaining": -2500000,
        "utilizationPercent": 125.0,
        "isOverBudget": true,
        "isNearLimit": false
      },
      {
        "month": 2,
        "monthName": "February",
        "budget": 10000000,
        "spent": 9500000,
        "remaining": 500000,
        "utilizationPercent": 95.0,
        "isOverBudget": false,
        "isNearLimit": true
      }
    ]
  }
}
```

### B. Automatic Scheduling (Recommended)

Untuk menjalankan otomatis, gunakan salah satu dari:

#### Option 1: Background Service (Recommended)

Buat `BudgetCheckBackgroundService.cs`:

```csharp
public class BudgetCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BudgetCheckBackgroundService> _logger;

    public BudgetCheckBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BudgetCheckBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var budgetJob = scope.ServiceProvider.GetRequiredService<IBudgetNotificationJob>();

                await budgetJob.RunAsync();

                // Run daily at 9 AM
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in budget check background service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
```

Register di `Program.cs`:

```csharp
builder.Services.AddHostedService<BudgetCheckBackgroundService>();
```

#### Option 2: Hangfire (Production Grade)

```csharp
// Install: dotnet add package Hangfire.AspNetCore
// Install: dotnet add package Hangfire.SqlServer

// In Program.cs
builder.Services.AddHangfire(config => config
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// Schedule job
app.UseHangfireDashboard();
RecurringJob.AddOrUpdate<IBudgetNotificationJob>(
    "budget-overbudget-check",
    job => job.RunAsync(),
    Cron.Daily(9)); // Every day at 9 AM
```

#### Option 3: CRON Job (Linux/macOS)

```bash
# Edit crontab
crontab -e

# Add this line (run daily at 9 AM)
0 9 * * * curl -X POST http://localhost:5008/api/jobs/budget-overbudget \
  -H "Authorization: Bearer YOUR_TOKEN"
```

#### Option 4: Windows Task Scheduler

1. Open Task Scheduler
2. Create Basic Task
3. Trigger: Daily at 9:00 AM
4. Action: Start a program
5. Program: `curl`
6. Arguments: `-X POST http://localhost:5008/api/jobs/budget-overbudget -H "Authorization: Bearer YOUR_TOKEN"`

## 📧 Email Template

Alert email menggunakan template dengan informasi:

- Period (Monthly/Yearly)
- Budget amount
- Spent amount
- Over budget amount & percentage
- Budget utilization percentage
- Action button "View Budget Details"

## 🔔 In-App Notification

Notifikasi muncul di:

- Notification bell icon (header)
- Notification page (`/Notification`)

Format:

```
⚠️ BUDGET OVERRUN ALERT - January 2026

Budget Exceeded!
Period: January 2026
Budget: 10,000,000.00
Spent: 12,500,000.00
Over Budget: 2,500,000.00 (25.0%)
Utilization: 125.0%

Immediate action required!
```

## 📊 Dashboard Integration

Untuk menampilkan status budget di dashboard:

```csharp
// In DashboardController or Dashboard page
var budgetStatus = await _budgetJob.GetBudgetStatusAsync(DateTime.Now.Year);

if (budgetStatus.IsOverBudget)
{
    // Show warning banner
    ViewBag.BudgetAlert = $"⚠️ Budget exceeded by {budgetStatus.TotalSpent - budgetStatus.TotalBudget:N2}";
}

// Check monthly status for current month
var currentMonth = budgetStatus.MonthlyStatus
    .FirstOrDefault(m => m.Month == DateTime.Now.Month);

if (currentMonth?.IsOverBudget == true)
{
    ViewBag.MonthlyAlert = $"⚠️ This month's budget exceeded by {currentMonth.Spent - currentMonth.Budget:N2}";
}
```

## 🛡️ Security

- **Authorization**: Hanya SUPER_ADMIN dan ADMIN yang bisa:
  - Trigger manual check
  - View budget status API
  - Menerima alert notifications

- **Email**: Alert hanya dikirim ke users dengan role SUPER_ADMIN atau ADMIN

## 🧪 Testing

### Test Manual Trigger

```bash
# Login sebagai admin terlebih dahulu
curl -X POST http://localhost:5008/api/jobs/budget-overbudget \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

### Test Budget Status

```bash
curl -X GET http://localhost:5008/api/jobs/budget-status/2026 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Create Test Overbudget Scenario

1. Login sebagai admin
2. Go to Budget page
3. Set monthly budget: Rp 10,000,000
4. Create invoices with total > Rp 10,000,000 untuk bulan ini
5. Run manual trigger: `POST /api/jobs/budget-overbudget`
6. Check notifications & email

## 📝 Audit Trail

Setiap alert yang dikirim akan tercatat di audit log:

- Entity: "Budget"
- Action: "BUDGET_OVERRUN_ALERT"
- Details: Year, Month, Excess amount, Excess percentage

## 💡 Tips

1. **Schedule Wisely**: Jalankan di pagi hari (9 AM) saat admin mulai bekerja
2. **Monitor Regularly**: Check budget status setiap hari untuk deteksi dini
3. **Set Realistic Budgets**: Pastikan budget realistic agar alert meaningful
4. **Review Alerts**: Jangan ignore alerts, review dan take action
5. **Adjust Budget**: Jika sering overbudget, review dan adjust budget planning

## 🔗 Related Documentation

- [Budget Feature Guide](BUDGET_FEATURE.md)
- [Notification Guide](NOTIFICATION_GUIDE.md)
- [Email System Guide](EMAIL_USAGE_GUIDE.md)
- [Email Templates Guide](EMAIL_TEMPLATES_GUIDE.md)

## 📞 Support

Jika ada masalah dengan budget alert system:

1. Check application logs
2. Verify budget configuration exists for current year
3. Check email SMTP configuration
4. Verify user roles (SUPER_ADMIN/ADMIN)
5. Check audit logs for alert history

---

**Last Updated:** January 31, 2026
**Version:** 1.0.0
