using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Master;

public static class InvoiceProgressStatusMappings
{
    public static readonly Expression<Func<InvoiceProgressStatuses, InvoiceProgressStatusDto>> ToDtoExpr =
        x => new InvoiceProgressStatusDto
        {
            ProgressStatusId = x.ProgressStatusId,
            Code = x.Code,
            Name = x.Name,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };

    public static InvoiceProgressStatusDto ToDto(this InvoiceProgressStatuses x)
    {
        return new InvoiceProgressStatusDto
        {
            ProgressStatusId = x.ProgressStatusId,
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
