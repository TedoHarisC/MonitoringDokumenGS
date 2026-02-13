namespace MonitoringDokumenGS.Interfaces
{
    public interface IBudgetNotificationJob
    {
        /// <summary>
        /// Check for overbudget conditions and send notifications
        /// </summary>
        Task RunAsync();

        /// <summary>
        /// Get current budget status for a specific year
        /// </summary>
        Task<BudgetOverviewResult> GetBudgetStatusAsync(int year);

        /// <summary>
        /// Check budget immediately after invoice change (real-time check)
        /// </summary>
        Task CheckBudgetOnInvoiceChangeAsync(int year, int month);
    }

    public class BudgetOverviewResult
    {
        public int Year { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal MonthlyBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal BudgetUtilizationPercent { get; set; }
        public bool IsOverBudget { get; set; }
        public List<MonthlyBudgetStatus> MonthlyStatus { get; set; } = new();
    }

    public class MonthlyBudgetStatus
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal UtilizationPercent { get; set; }
        public bool IsOverBudget { get; set; }
        public bool IsNearLimit { get; set; } // > 90%
    }
}
