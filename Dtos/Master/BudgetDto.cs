using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Dtos
{
    public class BudgetDto
    {
        [Key]
        public Guid BudgetId { get; set; }
        public int Year { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal MonthlyBudget { get; set; }
        public string TypeBudget { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}