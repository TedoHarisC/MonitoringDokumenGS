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

    public InvoiceNotificationBackgroundService(IServiceProvider serviceProvider, ILogger<InvoiceNotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Invoice Notification Background Service started");
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
                using var scope = _serviceProvider.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<IInvoiceNotificationJob>();
                await job.RunAsync();
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

    private DateTime GetNextRunTime(DateTime now)
    {
        var scheduledTime = now.Date.Add(_runTime);
        if (now >= scheduledTime)
            scheduledTime = scheduledTime.AddDays(1);
        return scheduledTime;
    }
}
