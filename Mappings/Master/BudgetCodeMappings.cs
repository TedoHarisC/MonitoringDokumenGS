using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;

namespace MonitoringDokumenGS.Mappings.Master;

public static class BudgetCodeMappings
{
    public static readonly Expression<Func<BudgetCode, BudgetCodeDto>> ToDtoExpr =
        x => new BudgetCodeDto
        {
            BudgetCodeId = x.BudgetCodeId,
            Code = x.Code,
            Description = x.Description,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
        };

    public static BudgetCodeDto ToDto(this BudgetCode x)
    {
        return new BudgetCodeDto
        {
            BudgetCodeId = x.BudgetCodeId,
            Code = x.Code,
            Description = x.Description,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
        };
    }
}
