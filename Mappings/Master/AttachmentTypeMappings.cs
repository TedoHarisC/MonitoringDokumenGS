using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Master;

public static class AttachmentTypeMappings
{
    public static readonly Expression<Func<AttachmentTypes, AttachmentTypeDto>> ToDtoExpr =
        x => new AttachmentTypeDto
        {
            AttachmentTypeId = x.AttachmentTypeId,
            Code = x.Code,
            Name = x.Name,
            AppliesTo = x.AppliesTo,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };

    public static AttachmentTypeDto ToDto(this AttachmentTypes x)
    {
        return new AttachmentTypeDto
        {
            AttachmentTypeId = x.AttachmentTypeId,
            Code = x.Code,
            Name = x.Name,
            AppliesTo = x.AppliesTo,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };
    }
}
