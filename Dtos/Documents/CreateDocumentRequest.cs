using System;
using System.ComponentModel.DataAnnotations;

namespace MonitoringDokumenGS.Dtos.Documents
{
    public class CreateDocumentRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}