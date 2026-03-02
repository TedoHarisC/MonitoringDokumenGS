using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Master;

public static class BudgetMappings
{
    public static readonly Expression<Func<Budget, BudgetDto>> ToDtoExpr =
        x => new BudgetDto
        {
            BudgetId = x.BudgetId,
            Year = x.Year,
            BudgetCodeId = x.BudgetCodeId,
            NoCoa = x.NoCoa,
            TypeBudget = x.TypeBudget,
            Activity = x.Activity,
            TotalBudget = x.TotalBudget,
            MonthlyBudget = x.MonthlyBudget,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy
        };

    public static BudgetDto ToDto(this Budget x)
    {
        return new BudgetDto
        {
            BudgetId = x.BudgetId,
            Year = x.Year,
            BudgetCodeId = x.BudgetCodeId,
            NoCoa = x.NoCoa,
            TypeBudget = x.TypeBudget,
            Activity = x.Activity,
            TotalBudget = x.TotalBudget,
            MonthlyBudget = x.MonthlyBudget,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy
        };
    }
}
