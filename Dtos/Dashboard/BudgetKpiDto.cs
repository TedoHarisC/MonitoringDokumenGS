namespace MonitoringDokumenGS.Dtos.Dashboard
{
    public class BudgetKpiDto
    {
        public string VendorName { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal Realisasi { get; set; }
        public decimal SisaBudget { get; set; }
        public decimal PersentaseSerapan { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public string TrafficLight { get; set; } = "green"; // green, yellow, red
    }

    public class MonthlyRealisasiDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Realisasi { get; set; }
        public decimal Budget { get; set; }
    }

    public class BudgetSummaryDto
    {
        public decimal TotalBudget { get; set; }
        public decimal TotalRealisasi { get; set; }
        public decimal TotalSisaBudget { get; set; }
        public decimal OverallPersentaseSerapan { get; set; }
        public string OverallTrafficLight { get; set; } = "green";
    }
}
