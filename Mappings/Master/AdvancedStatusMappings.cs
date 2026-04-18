using System;
using System.Linq.Expressions;
using MonitoringDokumenGS.Dtos.Master;
using MonitoringDokumenGS.Models.Master;

namespace MonitoringDokumenGS.Mappings.Master
{
    public static class AdvancedStatusMappings
    {
        public static readonly Expression<Func<AdvancedStatus, AdvancedStatusDto>> ToDtoExpr = x => new AdvancedStatusDto
        {
            AdvancedStatusId = x.AdvancedStatusesId,
            Code = x.Code,
            Name = x.Name
        };

        public static AdvancedStatusDto ToDto(AdvancedStatus x) => new AdvancedStatusDto
        {
            AdvancedStatusId = x.AdvancedStatusesId,
            Code = x.Code,
            Name = x.Name
        };
    }
}
