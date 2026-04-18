using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Models.Master;

namespace MonitoringDokumenGS.Mappings.Master
{
    public static class BiayaRealisasiStatusMappings
    {
        public static readonly Expression<Func<BiayaRealisasiStatus, BiayaRealisasiStatusDto>> ToDtoExpr = x => new BiayaRealisasiStatusDto
        {
            BiayaRealisasiStatusId = x.BiayaRealisasiStatusesId,
            Code = x.Code,
            Name = x.Name
        };

        public static BiayaRealisasiStatusDto ToDto(BiayaRealisasiStatus x) => new BiayaRealisasiStatusDto
        {
            BiayaRealisasiStatusId = x.BiayaRealisasiStatusesId,
            Code = x.Code,
            Name = x.Name
        };
    }
}
