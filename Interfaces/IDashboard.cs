namespace MonitoringDokumenGS.Interfaces
{
    public interface IDashboard
    {
        Task<IEnumerable<DashboardBudgetMonthlyDto>> GetMonthlyBudgetAsync(int year);
    }
}