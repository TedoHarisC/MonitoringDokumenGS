using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class Budget
    {
        [Key]
        public Guid BudgetId { get; set; }
        public int Year { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal MonthlyBudget { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}