using System.Collections.Generic;

namespace MonitoringDokumenGS.Dtos.Dashboard
{
    public class InvoiceStatusCountDto
    {
        public int ProgressStatusId { get; set; }
        public string ProgressStatusName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class InvoiceStatusSummaryDto
    {
        public List<InvoiceStatusCountDto> StatusCounts { get; set; } = new();
    }
}
