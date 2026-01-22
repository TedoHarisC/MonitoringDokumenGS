using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Master;

public static class VendorCategoryMappings
{
    public static readonly Expression<Func<VendorCategory, VendorCategoryDto>> ToDtoExpr =
        x => new VendorCategoryDto
        {
            VendorCategoryId = x.VendorCategoryId,
            Name = x.Name,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };

    public static VendorCategoryDto ToDto(this VendorCategory x)
    {
        return new VendorCategoryDto
        {
            VendorCategoryId = x.VendorCategoryId,
            Name = x.Name,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted
        };
    }
}
