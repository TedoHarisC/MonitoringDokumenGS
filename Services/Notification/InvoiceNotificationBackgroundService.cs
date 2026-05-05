using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class InvoiceNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InvoiceNotificationBackgroundService> _logger;
    private readonly TimeSpan _runTime = new TimeSpan(7, 0, 0); // 07:00 AM
    private DateTime? _lastRunDate;
    private static readonly string _lastRunFilePath = Path.Combine(AppContext.BaseDirectory, "invoice-notification-lastrun.txt");

    public InvoiceNotificationBackgroundService(IServiceProvider serviceProvider, ILogger<InvoiceNotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Invoice Notification Background Service started");

        await TryRunCatchUpAtStartupAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var nextRun = GetNextRunTime(now);
                var delay = nextRun - now;
                _logger.LogInformation("Invoice notification scheduled for {NextRun}. Waiting {Hours}h {Minutes}m", nextRun, (int)delay.TotalHours, delay.Minutes);
                await Task.Delay(delay, stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                    break;
                await ExecuteJobOncePerDayAsync(DateTime.Now.Date, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Invoice Notification Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Invoice Notification Background Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
        _logger.LogInformation("Invoice Notification Background Service stopped");
    }

    private async Task TryRunCatchUpAtStartupAsync(CancellationToken stoppingToken)
    {
        var now = DateTime.Now;
        var today = now.Date;
        var scheduledTimeToday = today.Add(_runTime);
        var catchUpDeadline = scheduledTimeToday.AddMinutes(30); // hanya catch-up dalam 30 menit setelah 07:00

        if (!IsTriggerDay(today.Day))
        {
            _logger.LogInformation("Invoice catch-up skipped at startup: today is not a trigger day");
            return;
        }

        if (now < scheduledTimeToday)
        {
            _logger.LogInformation("Invoice catch-up skipped at startup: current time is before scheduled run time {RunTime}", scheduledTimeToday);
            return;
        }

        if (now > catchUpDeadline)
        {
            _logger.LogInformation("Invoice catch-up skipped at startup: service started at {Now}, outside 30-minute catch-up window (07:00-07:30). Deploy/restart setelah itu tidak akan trigger email.", now);
            return;
        }

        var persistedDate = ReadLastRunDate();
        if (persistedDate.HasValue && persistedDate.Value == today)
        {
            _logger.LogInformation("Invoice catch-up skipped: already executed for date {Date} (persisted guard)", today);
            return;
        }

        _logger.LogInformation("Invoice catch-up will run now. Startup time is within catch-up window (07:00-07:30).");
        await ExecuteJobOncePerDayAsync(today, stoppingToken);
    }

    private async Task ExecuteJobOncePerDayAsync(DateTime runDate, CancellationToken stoppingToken)
    {
        if (!IsTriggerDay(runDate.Day))
        {
            _logger.LogInformation("Invoice job skipped by background service: {RunDate} is not a trigger day", runDate);
            return;
        }

        var persistedDate = ReadLastRunDate();
        if (persistedDate.HasValue && persistedDate.Value == runDate)
        {
            _logger.LogInformation("Invoice job skipped: already executed for date {RunDate} (persisted guard)", runDate);
            return;
        }

        if (_lastRunDate.HasValue && _lastRunDate.Value == runDate)
        {
            _logger.LogInformation("Invoice job skipped: already executed for date {RunDate} (memory guard)", runDate);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<IInvoiceNotificationJob>();
        await job.RunAsync();
        _lastRunDate = runDate;
        WriteLastRunDate(runDate);
    }

    private static DateTime? ReadLastRunDate()
    {
        try
        {
            if (!File.Exists(_lastRunFilePath)) return null;
            var text = File.ReadAllText(_lastRunFilePath).Trim();
            if (DateTime.TryParse(text, out var date))
                return date.Date;
        }
        catch { }
        return null;
    }

    private static void WriteLastRunDate(DateTime date)
    {
        try
        {
            File.WriteAllText(_lastRunFilePath, date.Date.ToString("yyyy-MM-dd"));
        }
        catch { }
    }

    private static bool IsTriggerDay(int day)
    {
        return day == 1 || day == 5 || day == 7 || day == 14 || day == 21 || day == 28;
    }

    private DateTime GetNextRunTime(DateTime now)
    {
        var scheduledTime = now.Date.Add(_runTime);
        if (now >= scheduledTime)
            scheduledTime = scheduledTime.AddDays(1);
        return scheduledTime;
    }
}
