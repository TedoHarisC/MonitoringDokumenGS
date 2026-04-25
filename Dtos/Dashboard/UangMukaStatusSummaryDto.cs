using System.Collections.Generic;

namespace MonitoringDokumenGS.Dtos.Dashboard
{
    public class UangMukaStatusCountDto
    {
        public string StatusId { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class UangMukaStatusSummaryDto
    {
        public List<UangMukaStatusCountDto> StatusCounts { get; set; } = new();
    }
}
