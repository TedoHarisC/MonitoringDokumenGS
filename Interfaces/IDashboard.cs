using MonitoringDokumenGS.Dtos.Dashboard;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IDashboard
    {
        Task<IEnumerable<DashboardBudgetMonthlyDto>> GetMonthlyBudgetAsync(int year);
        Task<IEnumerable<TopVendorSpendDto>> GetTopVendorsAsync(int top = 10, int? year = null);
        Task<IEnumerable<BudgetKpiDto>> GetBudgetKpiByVendorAsync(int year);
        Task<BudgetSummaryDto> GetBudgetSummaryAsync(int year);
        Task<IEnumerable<MonthlyRealisasiDto>> GetMonthlyRealisasiAsync(int year);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}