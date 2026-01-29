namespace MonitoringDokumenGS.Interfaces
{
    public interface IDashboard
    {
        Task<IEnumerable<DashboardBudgetMonthlyDto>> GetMonthlyBudgetAsync(int year);
        Task<IEnumerable<TopVendorSpendDto>> GetTopVendorsAsync(int top = 10, int? year = null);
    }
}