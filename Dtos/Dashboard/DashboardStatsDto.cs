namespace MonitoringDokumenGS.Dtos.Dashboard
{
    public class DashboardStatsDto
    {
        public int ActiveContractsCount { get; set; }
        public int TotalInvoicesSubmitted { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public int ContractsExpiringSoon { get; set; } // Contracts expiring in next 30 days
    }
}
