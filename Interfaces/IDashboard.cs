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
        Task<OnTimeSubmissionKpiDto> GetVendorOnTimeSubmissionKpiAsync(int? year = null);
        Task<object> GetUserMonthlyInvoiceTrendAsync(Guid userId, int? year = null);
        Task<IEnumerable<ContractExpiringDto>> GetContractsExpiringSoonAsync(int days = 30);

        // Invoice status summary for dashboard
        Task<InvoiceStatusSummaryDto> GetInvoiceStatusSummaryAsync();
        // Uang Muka status summary for dashboard
        Task<UangMukaStatusSummaryDto> GetUangMukaStatusSummaryAsync(string jenis);
        // Uang Muka detail by jenis & status
        Task<IEnumerable<object>> GetUangMukaDetailAsync(string jenis, string status);
    }
}