using MonitoringDokumenGS.Interfaces;

namespace MonitoringDokumenGS.Services.Notification
{
    /// <summary>
    /// Background service that runs contract expiring check daily at 8:00 AM
    /// </summary>
    public class ContractExpiringBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ContractExpiringBackgroundService> _logger;
        private readonly TimeSpan _checkTime = new TimeSpan(8, 0, 0); // 8:00 AM

        public ContractExpiringBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ContractExpiringBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Contract Expiring Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = GetNextRunTime(now);
                    var delay = nextRun - now;

                    _logger.LogInformation(
                        "Contract expiring check scheduled for {NextRun}. Waiting {Hours}h {Minutes}m",
                        nextRun, (int)delay.TotalHours, delay.Minutes);

                    // Wait until next scheduled time
                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    // Execute contract expiring check
                    await RunContractCheckAsync();
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Contract Expiring Background Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Contract Expiring Background Service");

                    // Wait 5 minutes before retrying on error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Contract Expiring Background Service stopped");
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

        private async Task RunContractCheckAsync()
        {
            _logger.LogInformation("Starting scheduled contract expiring check at {Time}", DateTime.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var contractJob = scope.ServiceProvider.GetRequiredService<IContractNotificationJob>();

                await contractJob.RunAsync();

                _logger.LogInformation("Scheduled contract expiring check completed successfully at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running scheduled contract expiring check");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Contract Expiring Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
