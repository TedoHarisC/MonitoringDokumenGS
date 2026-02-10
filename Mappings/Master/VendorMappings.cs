using System;
using System.Linq;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Master;

public static class VendorMappings
{
    public static readonly Expression<Func<Vendor, VendorDto>> ToDtoExpr =
        x => new VendorDto
        {
            VendorId = x.VendorId,
            VendorCode = x.VendorCode,
            VendorName = x.VendorName,
            ShortName = x.ShortName,
            VendorCategoryId = x.VendorCategoryId,
            OwnerName = x.OwnerName,
            OwnerPhone = x.OwnerPhone,
            CompanyEmail = x.CompanyEmail,
            NPWP = x.NPWP,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted,
            VendorPics = x.VendorPics.Where(p => !p.IsDeleted).Select(p => new VendorPicDto
            {
                VendorPicId = p.VendorPicId,
                VendorId = p.VendorId,
                PicName = p.PicName,
                PicNumber = p.PicNumber,
                PicEmail = p.PicEmail
            }).ToList()
        };

    public static VendorDto ToDto(this Vendor x)
    {
        return new VendorDto
        {
            VendorId = x.VendorId,
            VendorCode = x.VendorCode,
            VendorName = x.VendorName,
            ShortName = x.ShortName,
            VendorCategoryId = x.VendorCategoryId,
            OwnerName = x.OwnerName,
            OwnerPhone = x.OwnerPhone,
            CompanyEmail = x.CompanyEmail,
            NPWP = x.NPWP,
            CreatedAt = x.CreatedAt,
            CreatedBy = x.CreatedBy,
            UpdatedAt = x.UpdatedAt,
            UpdatedBy = x.UpdatedBy,
            IsDeleted = x.IsDeleted,
            VendorPics = x.VendorPics?.Where(p => !p.IsDeleted).Select(p => p.ToDto()).ToList()
        };
    }
}
