using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Notification
{
    /// <summary>
    /// Background service that runs budget overbudget check daily at 9:00 AM
    /// </summary>
    public class BudgetCheckBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BudgetCheckBackgroundService> _logger;
        private readonly TimeSpan _checkTime = new TimeSpan(9, 0, 0); // 9:00 AM

        public BudgetCheckBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BudgetCheckBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Budget Check Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = GetNextRunTime(now);
                    var delay = nextRun - now;

                    _logger.LogInformation(
                        "Budget check scheduled for {NextRun}. Waiting {Hours}h {Minutes}m",
                        nextRun, (int)delay.TotalHours, delay.Minutes);

                    // Wait until next scheduled time
                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    // Execute budget check
                    await RunBudgetCheckAsync();
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Budget Check Background Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Budget Check Background Service");

                    // Wait 5 minutes before retrying on error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Budget Check Background Service stopped");
        }

        private DateTime GetNextRunTime(DateTime now)
        {
            var scheduledTime = now.Date.Add(_checkTime);

            // If scheduled time already passed today, schedule for tomorrow
            if (now >= scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            return scheduledTime;
        }

        private async Task RunBudgetCheckAsync()
        {
            _logger.LogInformation("Starting scheduled budget check at {Time}", DateTime.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var budgetJob = scope.ServiceProvider.GetRequiredService<IBudgetNotificationJob>();

                await budgetJob.RunAsync();

                _logger.LogInformation("Scheduled budget check completed successfully at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running scheduled budget check");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Budget Check Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
