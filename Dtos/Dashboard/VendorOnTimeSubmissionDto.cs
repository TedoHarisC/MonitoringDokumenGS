namespace MonitoringDokumenGS.Dtos.Dashboard
{
    public class VendorOnTimeSubmissionDto
    {
        public string VendorName { get; set; } = string.Empty;
        public int TotalInvoices { get; set; }
        public int OnTimeSubmissions { get; set; }
        public int LateSubmissions { get; set; }
        public decimal OnTimePercentage { get; set; }
        public string PerformanceStatus { get; set; } = string.Empty; // "Excellent", "Good", "Poor"
    }

    public class OnTimeSubmissionKpiDto
    {
        public int TotalInvoices { get; set; }
        public int OnTimeSubmissions { get; set; }
        public int LateSubmissions { get; set; }
        public decimal OnTimePercentage { get; set; }
        public List<VendorOnTimeSubmissionDto> VendorBreakdown { get; set; } = new();
    }
}
