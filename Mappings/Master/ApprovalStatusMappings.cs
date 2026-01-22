using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Master;

public static class ApprovalStatusMappings
{
    public static readonly Expression<Func<ApprovalStatus, ApprovalStatusDto>> ToDtoExpr =
        x => new ApprovalStatusDto
        {
            ApprovalStatusId = x.ApprovalStatusId,
            Code = x.Code,
            Name = x.Name,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };

    public static ApprovalStatusDto ToDto(this ApprovalStatus x)
    {
        return new ApprovalStatusDto
        {
            ApprovalStatusId = x.ApprovalStatusId,
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
