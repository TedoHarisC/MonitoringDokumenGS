using Microsoft.AspNetCore.Http;
using System;

namespace MonitoringDokumenGS.Dtos.Master
{
    public class UploadTemplateFileRequest
    {
        public IFormFile File { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Permission { get; set; } = default!;
        public string Module { get; set; } = "TemplateFiles";
        public int? Id { get; set; } // null = tambah, ada = update file
    }
}
