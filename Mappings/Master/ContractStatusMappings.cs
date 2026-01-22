using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Master;

public static class ContractStatusMappings
{
    public static readonly Expression<Func<ContractStatus, ContractStatusDto>> ToDtoExpr =
        x => new ContractStatusDto
        {
            ContractStatusId = x.ContractStatusId,
            Code = x.Code,
            Name = x.Name,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };

    public static ContractStatusDto ToDto(this ContractStatus x)
    {
        return new ContractStatusDto
        {
            ContractStatusId = x.ContractStatusId,
            Code = x.Code,
            Name = x.Name,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };
    }
}
