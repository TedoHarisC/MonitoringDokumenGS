public class DashboardBudgetMonthlyDto
{
    public int Year { get; set; }
    public int Month { get; set; }

    public decimal TotalBudget { get; set; }
    public decimal Realisasi { get; set; }
    public decimal SisaBudget { get; set; }

    public decimal SerapanPercent { get; set; }
    public decimal VarianceRp { get; set; }
    public decimal VariancePercent { get; set; }

    public string Status { get; set; } = string.Empty;
}
