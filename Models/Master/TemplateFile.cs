using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Models
{
    public class TemplateFile
    {
        [Key]
        public int id { get; set; }
        public required string Title { get; set; }
        public required string Permission { get; set; }
        public string FileName { get; set; } = default!;
        public string FilePath { get; set; } = default!;
        public int FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}