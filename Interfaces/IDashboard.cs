using MonitoringDokumenGS.Dtos.Dashboard;

namespace MonitoringDokumenGS.Interfaces
{
    public interface IDashboard
    {
        Task<IEnumerable<object>> GetContractsByStatusAsync(string status);
        Task<IEnumerable<DashboardBudgetMonthlyDto>> GetMonthlyBudgetAsync(int year);
        Task<IEnumerable<TopVendorSpendDto>> GetTopVendorsAsync(int top = 10, int? year = null);
        Task<IEnumerable<BudgetCoaPerformanceDto>> GetBudgetCoaPerformanceAsync(int year, DateTime? startDate = null, DateTime? endDate = null, List<Guid>? budgetCodeIds = null, List<int>? coaTextIds = null);
        Task<BudgetSummaryDto> GetBudgetSummaryAsync(int year, Guid? budgetCodeId = null, int? vendorCategoryId = null);
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