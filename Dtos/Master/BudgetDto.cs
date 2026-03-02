using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Dtos
{
    public class BudgetDto
    {
        [Key]
        public Guid BudgetId { get; set; }
        public int Year { get; set; }
        public Guid? BudgetCodeId { get; set; }
        public string? BudgetCodeLabel { get; set; }
        public string NoCoa { get; set; } = string.Empty;
        public string TypeBudget { get; set; } = string.Empty;
        public string Activity { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal MonthlyBudget { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}