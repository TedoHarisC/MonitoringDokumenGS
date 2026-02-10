using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Models;

namespace MonitoringDokumenGS.Mappings.Master;

public static class VendorPicMappings
{
    public static VendorPicDto ToDto(this VendorPics x)
    {
        return new VendorPicDto
        {
            VendorPicId = x.VendorPicId,
            VendorId = x.VendorId,
            PicName = x.PicName,
            PicNumber = x.PicNumber,
            PicEmail = x.PicEmail
        };
    }
}
