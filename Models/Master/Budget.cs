using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class Budget
    {
        [Key]
        public Guid BudgetId { get; set; }
        public int Year { get; set; }
        public Guid? BudgetCodeId { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal MonthlyBudget { get; set; }
        public string NoCoa { get; set; } = string.Empty; // COA Number
        public string TypeBudget { get; set; } = string.Empty; // COA Text
        public string Activity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        // Navigation property
        public BudgetCode? BudgetCode { get; set; }
    }
}