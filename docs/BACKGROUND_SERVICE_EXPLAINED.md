# 📚 Background Service - Penjelasan Lengkap

## 🎯 Apa itu BackgroundService?

**BackgroundService** adalah base class dari .NET Core untuk membuat **long-running background tasks** yang berjalan di latar belakang aplikasi.

### **Analogi Sederhana:**

```
Aplikasi Web = Restoran
│
├─ Controllers = Pelayan (melayani customer request)
│
└─ BackgroundService = Chef (kerja terus di belakang, tidak peduli ada customer atau tidak)
```

---

## 🏗️ Cara Kerja BackgroundService

### **Lifecycle:**

```
1. Application Start
   ↓
2. BackgroundService.ExecuteAsync() dipanggil
   ↓
3. Loop terus berjalan (while loop)
   ↓
4. Execution logic (kirim email, check data, dll)
   ↓
5. Wait/Delay sampai waktu berikutnya
   ↓
6. Kembali ke step 3 (loop lagi)
   ↓
7. Application Stop → StopAsync() dipanggil
```

### **Thread Model:**

- Background service berjalan di **thread terpisah**
- **TIDAK blocking** main thread (HTTP requests tetap lancar)
- Berjalan **parallel** dengan web requests

---

## 📝 Cara Membuat BackgroundService - Step by Step

### **Step 1: Create Class yang Inherit BackgroundService**

```csharp
using Microsoft.Extensions.Hosting;

public class MyBackgroundService : BackgroundService
{
    private readonly ILogger<MyBackgroundService> _logger;

    public MyBackgroundService(ILogger<MyBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Logic di sini
    }
}
```

**Penjelasan:**

- `BackgroundService` = base class dari Microsoft.Extensions.Hosting
- `ExecuteAsync()` = method abstract yang HARUS diimplementasi
- `CancellationToken` = signal untuk stop service (saat app shutdown)

### **Step 2: Implement ExecuteAsync dengan Loop**

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("My Background Service started");

    // Loop sampai aplikasi di-stop
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            // DO YOUR WORK HERE
            _logger.LogInformation("Doing background work at {Time}", DateTime.Now);

            await DoWorkAsync();

            // WAIT before next execution
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Normal saat app shutdown
            _logger.LogInformation("Service is stopping");
            break;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background service");

            // Wait sebentar sebelum retry
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    _logger.LogInformation("My Background Service stopped");
}

private async Task DoWorkAsync()
{
    // Your actual work
    await Task.CompletedTask;
}
```

**Penjelasan:**

- `while (!stoppingToken.IsCancellationRequested)` = loop terus sampai app stop
- `await Task.Delay()` = tunggu X waktu sebelum run lagi
- `try-catch` = handle error tanpa crash service
- `TaskCanceledException` = normal exception saat shutdown

### **Step 3: Register di Program.cs**

```csharp
// In Program.cs
builder.Services.AddHostedService<MyBackgroundService>();
```

**Penjelasan:**

- `AddHostedService<T>()` = register background service
- Service akan **auto-start** saat aplikasi start
- Service akan **auto-stop** saat aplikasi stop

---

## 🎨 Pattern untuk Scheduled Tasks

### **Pattern 1: Simple Interval (Setiap X menit)**

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await DoWorkAsync();

        // Run every 5 minutes
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
    }
}
```

### **Pattern 2: Specific Time Daily (Seperti Budget/Contract Job)**

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var now = DateTime.Now;
        var nextRun = CalculateNextRunTime(now);
        var delay = nextRun - now;

        _logger.LogInformation("Next run scheduled at {NextRun}", nextRun);

        // Wait until scheduled time
        await Task.Delay(delay, stoppingToken);

        if (!stoppingToken.IsCancellationRequested)
        {
            await DoWorkAsync();
        }
    }
}

private DateTime CalculateNextRunTime(DateTime now)
{
    var scheduledTime = new TimeSpan(9, 0, 0); // 9:00 AM
    var nextRun = now.Date.Add(scheduledTime);

    // If already passed today, schedule for tomorrow
    if (now >= nextRun)
    {
        nextRun = nextRun.AddDays(1);
    }

    return nextRun;
}
```

**Ini pattern yang dipakai di BudgetCheckBackgroundService & ContractExpiringBackgroundService!**

### **Pattern 3: Hourly at Specific Minute**

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var now = DateTime.Now;
        var nextHour = now.Date.AddHours(now.Hour + 1); // Next hour
        var nextRun = nextHour.AddMinutes(30); // At :30 minutes

        var delay = nextRun - now;

        await Task.Delay(delay, stoppingToken);

        if (!stoppingToken.IsCancellationRequested)
        {
            await DoWorkAsync();
        }
    }
}
```

### **Pattern 4: Continuous Processing dengan Queue**

```csharp
private readonly Queue<WorkItem> _workQueue = new();

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        if (_workQueue.TryDequeue(out var workItem))
        {
            await ProcessWorkItemAsync(workItem);
        }
        else
        {
            // No work, wait a bit
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
```

---

## ⚙️ Menggunakan Dependency Injection

### **Problem: Scoped Services**

BackgroundService adalah **Singleton**, tapi banyak services (DbContext, dll) adalah **Scoped**.

**SALAH (Akan Error!):**

```csharp
public class MyBackgroundService : BackgroundService
{
    private readonly ApplicationDBContext _context; // ❌ SALAH!

    public MyBackgroundService(ApplicationDBContext context)
    {
        _context = context; // ❌ Scoped service di Singleton!
    }
}
```

**BENAR (Create Scope):**

```csharp
public class MyBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider; // ✅ Inject ServiceProvider

    public MyBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Create scope untuk setiap execution
            using var scope = _serviceProvider.CreateScope();

            // Get scoped services dari scope
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Do work
            await DoWorkAsync(context, emailService);

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            // Scope disposed di sini (DbContext juga disposed)
        }
    }
}
```

**Kenapa Perlu Scope?**

- DbContext harus di-dispose setelah use
- Scoped services punya lifetime lebih pendek
- Memory leak prevention

---

## 🎯 Contoh Real World

### **Example 1: Email Queue Processor**

```csharp
public class EmailQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailQueueProcessor> _logger;

    public EmailQueueProcessor(
        IServiceProvider serviceProvider,
        ILogger<EmailQueueProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Queue Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

                // Get pending emails from queue
                var pendingEmails = await context.EmailQueue
                    .Where(e => !e.IsSent && e.RetryCount < 3)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                if (pendingEmails.Any())
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    foreach (var email in pendingEmails)
                    {
                        try
                        {
                            await emailService.SendAsync(email.To, email.Subject, email.Body);

                            email.IsSent = true;
                            email.SentAt = DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            email.RetryCount++;
                            email.LastError = ex.Message;
                            _logger.LogError(ex, "Failed to send email {EmailId}", email.Id);
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Processed {Count} emails", pendingEmails.Count);
                }

                // Check every 30 seconds
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in email queue processor");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Email Queue Processor stopped");
    }
}
```

### **Example 2: Data Cleanup Job**

```csharp
public class DataCleanupJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataCleanupJob> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1).AddHours(2); // Tomorrow 2 AM

            await Task.Delay(nextRun - now, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await CleanupDataAsync();
            }
        }
    }

    private async Task CleanupDataAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Delete old logs (older than 90 days)
        var cutoffDate = DateTime.Now.AddDays(-90);
        var oldLogs = await context.AuditLogs
            .Where(l => l.CreatedAt < cutoffDate)
            .ToListAsync();

        context.AuditLogs.RemoveRange(oldLogs);
        await context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} old logs", oldLogs.Count);
    }
}
```

### **Example 3: Cache Refresher**

```csharp
public class CacheRefresherService : BackgroundService
{
    private readonly IMemoryCache _cache;
    private readonly IServiceProvider _serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RefreshCacheAsync();

            // Refresh every 10 minutes
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }

    private async Task RefreshCacheAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        // Refresh frequently accessed data
        var activeUsers = await context.Users
            .Where(u => u.isActive && !u.isDeleted)
            .ToListAsync();

        _cache.Set("ActiveUsers", activeUsers, TimeSpan.FromMinutes(15));
    }
}
```

---

## 🛡️ Best Practices

### **1. Always Use Try-Catch**

```csharp
while (!stoppingToken.IsCancellationRequested)
{
    try
    {
        await DoWorkAsync();
        await Task.Delay(interval, stoppingToken);
    }
    catch (TaskCanceledException)
    {
        // Normal shutdown
        break;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in background service");
        // Wait before retry
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}
```

**Kenapa?**

- Prevent service crash
- Graceful error handling
- Auto-retry capability

### **2. Use CancellationToken**

```csharp
// ✅ GOOD
await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

// ❌ BAD
await Task.Delay(TimeSpan.FromMinutes(5)); // Ignore stoppingToken
```

**Kenapa?**

- Graceful shutdown
- App tidak hang saat stop

### **3. Create Scope for Scoped Services**

```csharp
using var scope = _serviceProvider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IMyService>();
```

**Kenapa?**

- Proper lifetime management
- Memory leak prevention
- DbContext disposal

### **4. Log Important Events**

```csharp
_logger.LogInformation("Service started");
_logger.LogInformation("Processing {Count} items", count);
_logger.LogError(ex, "Error processing item {Id}", itemId);
_logger.LogInformation("Service stopped");
```

**Kenapa?**

- Debugging di production
- Monitor service health
- Audit trail

### **5. Override StopAsync for Cleanup (Optional)**

```csharp
public override async Task StopAsync(CancellationToken cancellationToken)
{
    _logger.LogInformation("Service is stopping, cleaning up...");

    // Cleanup resources
    // Close connections
    // Save state

    await base.StopAsync(cancellationToken);
}
```

---

## ⚡ Performance Tips

### **1. Batch Processing**

```csharp
// ✅ GOOD - Process in batches
var items = await context.Items.Take(100).ToListAsync();

// ❌ BAD - Process all (memory intensive)
var items = await context.Items.ToListAsync();
```

### **2. Async All the Way**

```csharp
// ✅ GOOD
await Task.Delay(interval, stoppingToken);
await context.SaveChangesAsync(stoppingToken);

// ❌ BAD
Thread.Sleep(interval); // Blocks thread!
context.SaveChanges(); // Blocking!
```

### **3. Dispose Resources**

```csharp
using var scope = _serviceProvider.CreateScope();
// Resources auto-disposed
```

---

## 📊 Monitoring BackgroundService

### **Check if Running:**

```csharp
// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.Now
}));
```

### **View Logs:**

```csharp
// Development
_logger.LogInformation("Service running at {Time}", DateTime.Now);

// Production - use proper logging provider
// (Serilog, NLog, Application Insights)
```

---

## 🎓 Summary

### **BackgroundService adalah:**

- ✅ Built-in .NET Core (tidak perlu package)
- ✅ Berjalan di background thread terpisah
- ✅ Auto-start saat app start
- ✅ Cocok untuk scheduled tasks
- ✅ Lightweight & simple

### **Cara Membuat:**

1. **Inherit** dari `BackgroundService`
2. **Implement** `ExecuteAsync()` dengan while loop
3. **Register** dengan `AddHostedService<T>()`
4. **Done!** Auto-start & auto-stop

### **Pattern Umum:**

- Loop dengan `while (!stoppingToken.IsCancellationRequested)`
- `Task.Delay()` untuk interval/scheduling
- `CreateScope()` untuk scoped services
- Try-catch untuk error handling

### **Use Cases:**

- ✅ Scheduled tasks (daily, hourly)
- ✅ Queue processing
- ✅ Data cleanup
- ✅ Cache refresh
- ✅ Notification sender
- ✅ Health checks
- ✅ Log archiving

---

## 🚀 Next Steps

Sekarang Anda sudah paham BackgroundService! Bisa bikin untuk:

- Archive old data
- Generate reports
- Sync with external API
- Process file uploads
- Monitor system health
- Auto-backup database
- Dan banyak lagi!

**Simple, powerful, dan built-in!** 🎉
